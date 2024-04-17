using MyBox;
using System;
using System.Collections;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Msmp.Server;
using Msmp.Client;
using Msmp.Server.Packets.Customers;
using System.Linq;
using Random = UnityEngine.Random;
using Msmp.Utility;
using DG.Tweening;

namespace Msmp.Mono
{
    internal class NetworkedCustomer : MonoBehaviour
    {
        public Customer _customer;
        public Guid NetworkId { get; set; }

        private bool CantFindProduct = false;

        public bool IsShopping = false;

        public void Awake()
        {
            _customer = GetComponent<Customer>();   
        }

        public void SyncStartShopping(ItemQuantity shoppingList)
        {
            if(shoppingList == null || shoppingList.Products.Count <= 0)
            {
                Console.WriteLine("ShoppingList was empty or null");
            }

            _customer.ShoppingList = shoppingList;

            _customer.ShoppingList.Products[83] = 2;
            _customer.ShoppingList.Products[70] = 1;


            StartCoroutine(Shopping());
        }

        public IEnumerator Shopping()
        {
            ProcessShoppingList();

            yield break;
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
            Console.WriteLine("Walking around");
            List<Display> displays = Singleton<DisplayManager>.Instance.GetType().GetPrivateField<List<Display>>("m_Displays", _customer);
            Display display = displays[displayId];
            StartCoroutine("WalkAroundIdle", display);
        }

        private void ProcessShoppingList()
        {
            int productId = _customer.ShoppingList.Products.Keys.FirstOrDefault();

            if (productId == default)
            {
                Console.WriteLine("All products has been purchesed");

                if (_customer.GetType().GetPrivateField<bool>("IsShopping", _customer))
                {
                    Console.WriteLine("Customer is still shopping");
                    DisplayManager manager = Singleton<DisplayManager>.Instance;
                    List<Display> displays = manager.GetType().GetPrivateField<List<Display>>("m_Displays", _customer);
                    int randomDisplayIndex = Random.Range(0, displays.Count);

                    OutCustomerWalkAround outCustomerWalkAround = new OutCustomerWalkAround()
                    {
                        DisplaySlotId = randomDisplayIndex,
                        NetworkId = NetworkId
                    };

                    Packet packet = new Packet(PacketType.CustomerWalkAround, outCustomerWalkAround);

                    MsmpClient.Instance.SendPayload(packet);
                }
                else
                {
                    Console.WriteLine("Customer is not shopping");
                    if (!_customer.GetType().GetPrivateField<bool>("m_StartedShopping", _customer) || _customer.ShoppingList.Products.Count > 0)
                    {

                    }
                }

                GoToCheckout();
                return;
            }

            Console.WriteLine($"Process productId: {productId} count: {_customer.ShoppingList.Products[productId]}");

            if (MsmpClient.Instance.IsServer)
            {
                if (Singleton<InventoryManager>.Instance.IsProductDisplayed(productId))
                {
                    List<DisplaySlot> displaySlots = Singleton<DisplayManager>.Instance.GetDisplaySlots(productId, true);

                    if (CantFindProduct)
                    {
                        if(_customer.ShoppingCart.Products.Count > 0)
                        {
                            GoToCheckout();
                        }
                        else
                        {
                            FinishShopping();
                        }
                    }

                    if (CantFindProduct || displaySlots == null || displaySlots.Count <= 0)
                    {
                        ProcessShoppingList();

                        CantFindProduct = true;

                        return;
                    }

                    int displaySlotId = Random.Range(0, displaySlots.Count);

                    Vector2 m_ExtraPurchaseAmount = _customer.GetType().GetPrivateField<Vector2>("m_ExtraPurchaseAmount", _customer);
                    int purchaseChance = (int)Math.Floor(Singleton<PriceEvaluationManager>.Instance.PurchaseChance(productId));

                    OutCustomerTakeProductPacket outCustomerTakeProductPacket = new OutCustomerTakeProductPacket()
                    {
                        NetworkId = NetworkId,
                        DisplaySlotId = displaySlotId,
                        ProductId = productId,
                        PurchaseChance = purchaseChance,
                        IsExpensiveRandom = Random.Range(0, 100),
                        RandomMultiplier = (int)Math.Floor(Random.Range(m_ExtraPurchaseAmount.x, m_ExtraPurchaseAmount.y) * purchaseChance),
                        ProductCount = _customer.ShoppingList.Products[productId]
                    };

                    Console.WriteLine("Sending next product");

                    Packet packet = new Packet(PacketType.CustomerTakeProductFromDisplay, outCustomerTakeProductPacket);

                    MsmpClient.Instance.SendPayload(packet);
                }
                else
                {
                    /* Customer bought all products*/
                    if(_customer.ShoppingCart.Products.Count > 0)
                    {
                        GoToCheckout();
                    } /* Customer did not find any products*/
                    else
                    {
                        FinishShopping();
                    }
                }
            }
        }

        private void FinishShopping()
        {
            bool m_StartedShopping = _customer.GetType().GetPrivateField<bool>("m_StartedShopping", _customer);

            if (!m_StartedShopping && !Singleton<DisplayManager>.Instance.HasAnythingDisplayed)
            {
                Singleton<WarningSystem>.Instance.SpawnCustomerSpeech(CustomerSpeechType.CANT_FIND_ANYTHING, _customer.transform, Array.Empty<string>());
                Singleton<DailyStatisticsManager>.Instance.AddCouldntFindProduct();
            }
            _customer.GetType().SetPrivateField("m_IsSatisfiedCustomer", false, _customer);
            DOVirtual.DelayedCall(0.5f, delegate
            {
                Singleton<StoreLevelManager>.Instance.RemovePoint(_customer.GetType().GetPrivateField<int>("m_StorePointPenalty", _customer));
            }, true);
            _customer.GetType().GetMethod("FinishShopping", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_customer, new object[] { false });

        }

        public void SyncTakeProductFromDisplay(OutCustomerTakeProductPacket packet)
        {
            List<DisplaySlot> displaySlots = Singleton<DisplayManager>.Instance.GetDisplaySlots(packet.ProductId, true);

            if(displaySlots == null || displaySlots.Count == 0)
            {
                return;
            }

            if (displaySlots.Count != 0 && displaySlots != null)
            {
                StartCoroutine(TakeProductsFromDisplay(displaySlots[packet.DisplaySlotId], packet.ProductId, packet.PurchaseChance, packet.IsExpensiveRandom
                             , packet.RandomMultiplier, packet.ProductCount));
            }
        }

        private IEnumerator TakeProductsFromDisplay(DisplaySlot displaySlot, int productId,
            int purchaseChance, int isExpensiveRandom, int randomMultiplier, int productCount)
        {
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
                bool takeProduct = (bool)_customer.GetType().GetMethod("TakeProduct", BindingFlags.NonPublic | BindingFlags.Instance)
                      .Invoke(_customer, new object[2] { displaySlot, productId });

                if (takeProduct)
                {
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
