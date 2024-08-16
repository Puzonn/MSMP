using HarmonyLib;
using Msmp.Utility;
using MyBox;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;
using System.Diagnostics.Contracts;
using Msmp.Mono;

namespace Msmp.Patch.CheckoutPatch
{
    [HarmonyPatch(typeof(Checkout))]
    [HarmonyPatch("TryFinishingCashPayment")]
    internal class CheckoutTryFinishingCashPayment
    {
        [HarmonyPrefix]
        static bool Prefix(Checkout __instance)
        {
            List<Customer> m_Customers = __instance.GetType().GetPrivateField<List<Customer>>("m_Customers", __instance);
            CheckoutScreen m_CheckoutScreen = __instance.GetType().GetPrivateField<CheckoutScreen>("m_CheckoutScreen", __instance);
            CheckoutDrawer m_CheckoutDrawer = __instance.GetType().GetPrivateField<CheckoutDrawer>("m_CheckoutDrawer", __instance);
            Animator m_CashRegisterAnimator = __instance.GetType().GetPrivateField<Animator>("m_CashRegisterAnimator", __instance);
            float m_CorrectChange = __instance.GetType().GetPrivateField<float>("m_CorrectChange", __instance);
            int m_PointPenaltyOnLowChange = __instance.GetType().GetPrivateField<int>("m_PointPenaltyOnLowChange", __instance);
            int m_StorePointPerCheckout = __instance.GetType().GetPrivateField<int>("m_StorePointPerCheckout", __instance);
            float randomPointPenaltyChanceOnLowChange = Random.Range(0f, 100f);
            float m_CollectedChange = __instance.GetType().GetPrivateField<float>("m_CollectedChange", __instance);
            float m_PointPenaltyChanceOnLowChange = __instance.GetType().GetPrivateField<float>("m_PointPenaltyChanceOnLowChange", __instance);
            float m_ReceivedPayment = __instance.GetType().GetPrivateField<float>("m_ReceivedPayment", __instance);
            float m_TotalPrice = __instance.GetType().GetPrivateField<float>("m_TotalPrice", __instance);
            bool m_ReachedMinChange = GetReachedMinChange(m_CollectedChange, m_CorrectChange);
            bool m_CanApproveChange = GetApproveChance(m_CollectedChange, m_CorrectChange, m_TotalPrice);

            if (m_Customers.Count <= 0 || __instance.CurrentState != Checkout.State.PAYMENT_CASH || m_ReceivedPayment <= 0f)
            {
                return false;
            }
            if (!m_CanApproveChange)
            {
                Singleton<WarningSystem>.Instance.RaiseInteractionWarning(InteractionWarningType.INSUFFICIENT_CHANGE, Array.Empty<string>());
                return false;
            }

            bool shortchange = false;

            if (!m_ReachedMinChange && randomPointPenaltyChanceOnLowChange <= m_PointPenaltyChanceOnLowChange)
            {
                Singleton<WarningSystem>.Instance.SpawnCustomerSpeech(CustomerSpeechType.THIS_IS_THEFT, m_Customers[0].transform, Array.Empty<string>());
                Singleton<DailyStatisticsManager>.Instance.AddIncorrectChangeAmount();
                m_Customers[0].IsSatisfied = false;
                Singleton<StoreLevelManager>.Instance.RemovePoint(m_PointPenaltyOnLowChange);
                shortchange = true;
            }
            else
            {
                Singleton<StoreLevelManager>.Instance.AddPoint(m_StorePointPerCheckout);
            }
            Singleton<MoneyManager>.Instance.MoneyTransition(m_ReceivedPayment - m_CollectedChange, MoneyManager.TransitionType.CHECKOUT_INCOME);
            m_Customers[0].GetComponent<NetworkedCustomer>().FinishShopping(shortchange);
            __instance.Subscribe(m_Customers[0]);
            __instance.GetType().InvokePrivateMethod("ResetCashRegister", null, __instance);
            __instance.GetType().InvokePrivateMethod("ChangeState", new object[] { Checkout.State.IDLE }, __instance);
            m_CheckoutScreen.Clear();
            m_CashRegisterAnimator.SetBool("Open", false);
            Singleton<SFXManager>.Instance.PlayCashRegister(false);
            m_CheckoutDrawer.MoneyInteraction = false;
            __instance.GetType().SetPrivateField("m_CollectingChange", false, __instance);
            __instance.GetType().InvokePrivateMethod("AskForCustomer", null, __instance);
            if (Singleton<OnboardingManager>.Instance != null && Singleton<OnboardingManager>.Instance.Step == 11)
            {
                Singleton<OnboardingManager>.Instance.NextStep(0f, false);
            }
            Singleton<SaveManager>.Instance.Progression.CompletedCheckoutCount++;
            Action onCheckoutCompleted = Singleton<CheckoutManager>.Instance.onCheckoutCompleted;
            if (onCheckoutCompleted != null)
            {
                onCheckoutCompleted();
            }
            Singleton<SFXManager>.Instance.PlayCheckoutSFX();
            return false;
        }

        static bool GetApproveChance(float m_CollectedChange, float m_CorrectChange, float m_TotalPrice)
        {
            Console.WriteLine($"GetApproveChange {m_CollectedChange} {m_CorrectChange} {m_TotalPrice}");
            if (Math.Round(m_CollectedChange, 2) >= Math.Round(m_CorrectChange * 0.5f, 2))
            {
                return m_CorrectChange - m_CollectedChange <= m_TotalPrice * 0.5f;
            }

            return false;
        }

        static bool GetReachedMinChange(float m_CollectedChange, float m_CorrectChange)
        {
            Console.WriteLine($"MinChange {m_CollectedChange} {m_CollectedChange}");
            return (float)Math.Round(m_CollectedChange, 2) >= (float)Math.Round(m_CorrectChange, 2);
        }
    }
}
