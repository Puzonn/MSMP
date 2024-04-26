using MyBox;
using System;
using System.Collections;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Msmp.Server.Packets.Customers;
using Msmp.Utility;
using DG.Tweening;
using Msmp.Server.Models;
using System.Runtime.InteropServices;

namespace Msmp.Mono
{
    internal class NetworkedCustomer : MonoBehaviour
    {
        public Guid NetworkId { get; set; }

        private Customer _customer;
        /* TODO: Check TakeProductFromDisplay */
        private bool _isShopping = false;
        private bool _startedShopping = false;

        private DisplaySlot _randomWalkDisplaySlot = null;

        private List<ProcessedProduct> _processedProducts { get; set; }

        public void Awake()
        {
            _customer = GetComponent<Customer>();
        }

        public void SyncStartShopping(OutCustomerStartShopping packet)
        {
            _customer.ShoppingList = packet.ShoppingList;

            _processedProducts = packet.ProcessedProducts;
            _randomWalkDisplaySlot = DisplayUtility.GetDisplaySlot(packet.WalkRandomDisplay, packet.WalkRandomDisplaySlot);
            StartCoroutine(Shopping());
        }

        public IEnumerator Shopping()
        {
            yield return StartCoroutine(ProcessShoppingList());

            if (_isShopping)
            {
                yield return StartCoroutine(ProcessShoppingList());
                GoToCheckout();
            }
            else
            {
                if (!_startedShopping || _customer.ShoppingList.Products.Count > 0)
                {
                    yield return _customer.StartCoroutine("WalkAroundIdle", _randomWalkDisplaySlot);
                    yield return StartCoroutine(ProcessShoppingList());
                }

                if (_isShopping)
                {
                    GoToCheckout();
                }
                else
                {
                    if (!_startedShopping && !Singleton<DisplayManager>.Instance.HasAnythingDisplayed)
                    {
                        Singleton<WarningSystem>.Instance.SpawnCustomerSpeech(CustomerSpeechType.CANT_FIND_ANYTHING, base.transform, Array.Empty<string>());
                        Singleton<DailyStatisticsManager>.Instance.AddCouldntFindProduct();
                    }
                    _customer.GetType().SetPrivateField("m_IsSatisfiedCustomer", false, _customer);

                    DOVirtual.DelayedCall(0.5f, delegate
                    {
                        Singleton<StoreLevelManager>.Instance.RemovePoint(_customer.GetType().GetPrivateField<int>("m_StorePointPenalty", _customer));
                    }, true);

                    FinishShopping(false);
                }
            }
        }

        private void FinishShopping(bool shortchange = false)
        {
            _customer.GetType().GetMethod("CheckForProductsMissing", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(_customer, new object[] { shortchange });

            _customer.StartCoroutine("ExitStore");

            if (_customer.GetType().GetPrivateField<bool>("m_IsSatisfiedCustomer", _customer))
            {
                Singleton<DailyStatisticsManager>.Instance.AddSatisfiedCustomer();
            }

            Singleton<DailyStatisticsManager>.Instance.AddCustomer();
            CheckoutInteraction instance = CheckoutInteraction.Instance;

            var onCheckoutBoxed = Delegate.CreateDelegate(typeof(Action<Checkout>), null, _customer.GetType()
                .GetMethod("OnCheckoutBoxed", BindingFlags.Instance | BindingFlags.NonPublic));
            instance.onCheckoutBoxed = (Action<Checkout>)Delegate.Remove(instance.onCheckoutBoxed, onCheckoutBoxed);
        }

        private IEnumerator ProcessShoppingList()
        {
            foreach (ProcessedProduct product in _processedProducts)
            {
                if (product.IsProductDisplayed)
                {
                    while (_customer.ShoppingList.Products.ContainsKey(product.ProductId) && _customer.ShoppingList.Products[product.ProductId] > 0)
                    {
                        List<DisplaySlot> displaySlots = Singleton<DisplayManager>.Instance.GetDisplaySlots(product.ProductId, true);
                        if (displaySlots == null || displaySlots.Count <= 0)
                        {
                            break;
                        }

                        DisplaySlot targetDisplaySlot = displaySlots[product.DisplaySlotId];
                        _customer.GetType().SetPrivateField("m_TargetDisplay", targetDisplaySlot.Display, _customer);
                        yield return _customer.StartCoroutine("MoveTo", targetDisplaySlot.InteractionPosition);

                        /*TODO: Check if needed */
                        if (targetDisplaySlot != null)
                        {
                            yield return StartCoroutine(TakeProductsFromDisplay
                                (targetDisplaySlot, product.ProductId, product.PurchaseChance, 
                                product.ExpensiveRandom, product.RandomMultiplier, product.ProductCount));
                        }

                        targetDisplaySlot = null;
                    }
                }
            }
        }

        public void GoToCheckout()
        {
            Checkout availableCheckout = Singleton<CheckoutManager>.Instance.GetAvailableCheckout;

            List<Customer> m_Customers = availableCheckout.GetType().GetPrivateField<List<Customer>>("m_Customers", availableCheckout);

            Queue m_Queue = availableCheckout.GetType().GetPrivateField<Queue>("m_Queue", availableCheckout);

            if (availableCheckout != null)
            {
                m_Customers.Add(_customer);
                _customer.MoveCheckoutPosition(availableCheckout, m_Queue.GetQueuePosition(m_Customers.Count - 1), m_Customers.Count - 1 == 0);
                return;
            }

            Singleton<WarningSystem>.Instance.SpawnCustomerSpeech(CustomerSpeechType.FULL_CHECKOUTS, base.transform, Array.Empty<string>());
            Singleton<CheckoutManager>.Instance.m_CustomersAwaiting.Add(_customer);
        }

        public void SyncWalkAround(int displayId)
        {
            List<Display> displays = Singleton<DisplayManager>.Instance.GetType().GetPrivateField<List<Display>>("m_Displays", _customer);
            Display display = displays[displayId];
            StartCoroutine("WalkAroundIdle", display);
        }

        private IEnumerator TakeProductsFromDisplay(DisplaySlot displaySlot, int productId,
            int purchaseChance, int isExpensiveRandom, int randomMultiplier, int productCount)
        {
            if(_customer.GetType().GetPrivateField<Display>("m_TargetDisplay", _customer) == null)
            {
                Console.WriteLine($"{NetworkId} had m_TargetDisplay null, yielding break");
                yield break;
            }

            _startedShopping = true;

            /* TODO: Rotations dose not work, do them on CustomerComponent */
            transform.DOKill(false);
            Quaternion interactionRotation = displaySlot.InteractionRotation;
            transform.DORotateQuaternion(interactionRotation, 0.3f);

            yield return new WaitForSeconds(0.3f);

            _customer.GetType().SetPrivateField("m_TargetDisplay", displaySlot.Display, _customer);

            yield return _customer.StartCoroutine("MoveTo", displaySlot.InteractionPosition);

            ItemQuantity m_ShoppingList = _customer.GetType().GetPrivateField<ItemQuantity>("m_ShoppingList", _customer);
            ProductSO productSO = Singleton<IDManager>.Instance.ProductSO(productId);
            CustomerAnimator m_Animator = _customer.GetType().GetPrivateField<CustomerAnimator>("m_Animator", _customer);

            _customer.GetType().SetPrivateField("m_StartedShopping", true, _customer);

            if (purchaseChance <= 0f)
            {
                string text = productSO.ComplexName(Singleton<WarningSystem>.Instance.CustomerSpeechFontSize);
                Singleton<WarningSystem>.Instance.SpawnCustomerSpeech(CustomerSpeechType.EXPENSIVE, base.transform, new string[]
                {
                    text
                });

                _customer.GetType().SetPrivateField("m_IsSatisfiedCustomer", false, _customer);

                Singleton<DailyStatisticsManager>.Instance.AddExpensiveProducts();
                m_ShoppingList.Products.Remove(productId);
                m_Animator.ExpensiveProduct();
                yield return new WaitForSeconds(2f);
                yield break;
            }
            if (purchaseChance > 100f)
            {
                int value = Mathf.CeilToInt((float)m_ShoppingList.Products[productId] * randomMultiplier / 100f);
                m_ShoppingList.Products[productId] = value;
            }
            else if (purchaseChance < isExpensiveRandom)
            {
                string text2 = productSO.ComplexName(Singleton<WarningSystem>.Instance.CustomerSpeechFontSize);
                Singleton<WarningSystem>.Instance.SpawnCustomerSpeech(CustomerSpeechType.EXPENSIVE_FOR_ME, base.transform, new string[]
                {
                    text2
                });

                _customer.GetType().SetPrivateField("m_IsSatisfiedCustomer", false, _customer);

                Singleton<DailyStatisticsManager>.Instance.AddExpensiveProducts();
                m_ShoppingList.Products.Remove(productId);
                m_Animator.ExpensiveProduct();
                yield return new WaitForSeconds(2f);
                yield break;
            }

            while (productCount > 0 && displaySlot.HasProduct && displaySlot.ProductID == productId
                && _customer.GetType().GetPrivateField<Display>("m_TargetDisplay", _customer) != null)
            {
                productCount--;

                /*TODO: Seems like TakeProduct dose not set ShoppingCart to valid value or IsShopping is just bugged */
                bool takeProduct = (bool)_customer.GetType().GetMethod("TakeProduct", BindingFlags.NonPublic | BindingFlags.Instance)
                      .Invoke(_customer, new object[2] { displaySlot, productId });

                if (takeProduct)
                {
                    _isShopping = true;
                    m_Animator.PickUpProductFromDisplay(displaySlot.transform.position.y - transform.position.y);

                    _customer.GetType().GetField("m_IsPlayingAnimation", BindingFlags.NonPublic | BindingFlags.Instance)
                        .SetValue(_customer, true);

                    while (_customer.GetType().GetPrivateField<bool>("m_IsPlayingAnimation", _customer))
                    {
                        yield return null;
                    }

                    m_Animator.FinishAnimation(false);

                    if (!m_ShoppingList.Products.ContainsKey(productId))
                    {
                        break;
                    }
                }
            }

            /*TODO: Check if needed */
            _customer.ShoppingList.Products.Remove(productId);

            if (_customer.GetType().GetPrivateField<bool>("m_IsPickingUp", _customer))
            {
                m_Animator.FinishAnimation(true);
                _customer.GetType().SetPrivateField("m_IsPickingUp", false, _customer);
            }

            if (!Singleton<PriceManager>.Instance.HasPriceSetByPlayer(productId))
            {
                Singleton<WarningSystem>.Instance.RaiseNoProfitWarning();
            }

            _customer.GetType().SetPrivateField("m_TargetDisplay", null, _customer);

            ProcessShoppingList();
        }
    }
}
