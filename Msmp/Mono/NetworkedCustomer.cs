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
using Msmp.Server.Models;
using Lean.Pool;
using System.Runtime.InteropServices;

namespace Msmp.Mono
{
    internal class NetworkedCustomer : MonoBehaviour
    {
        public Customer _customer;
        public Guid NetworkId { get; set; }

        public bool IsShopping = false;

        public void Awake()
        {
            _customer = GetComponent<Customer>();   
        }

        public void StartShopping(ItemQuantity shoppingList)
        {
            if(shoppingList == null || shoppingList.Products.Count <= 0)
            {
                Console.WriteLine("ShoppingList was empty or null");
            }

            shoppingList.Products[83] = 2;

            _customer.ShoppingList = shoppingList;

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

            List<Customer> m_Customers = (List<Customer>)availableCheckout.GetType().GetField("m_Customers", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(availableCheckout);

            Queue m_Queue = (Queue)availableCheckout.GetType().GetField("m_Queue", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(availableCheckout);

            if (availableCheckout != null)
            {
                m_Customers.Add(_customer);
                _customer.MoveCheckoutPosition(availableCheckout, m_Queue.GetQueuePosition(m_Customers.Count - 1), m_Customers.Count - 1 == 0);
                return;
            }

            Singleton<WarningSystem>.Instance.SpawnCustomerSpeech(CustomerSpeechType.FULL_CHECKOUTS, base.transform, Array.Empty<string>());
            Singleton<CheckoutManager>.Instance.m_CustomersAwaiting.Add(_customer);
           // base.StartCoroutine(this.WaitForAvailableCheckout());
        }

        private void ProcessShoppingList()
        {
            Console.WriteLine("Processing ...");
            int productID = _customer.ShoppingList.Products.Keys.FirstOrDefault();

            if(productID == default)
            {
                Console.WriteLine("No products to process");

                GoToCheckout();

                return;
            }

            if (MsmpClient.Instance.IsServer)
            {
                if (Singleton<InventoryManager>.Instance.IsProductDisplayed(productID))
                {
                    List<DisplaySlot> displaySlots = Singleton<DisplayManager>.Instance.GetDisplaySlots(productID, true);

                    if (displaySlots == null || displaySlots.Count <= 0)
                    {
                        return;
                    }


                    int displaySlotId = Random.Range(0, displaySlots.Count);

                    OutCustomerTakeProductPacket outCustomerTakeProductPacket = new OutCustomerTakeProductPacket()
                    {
                        NetworkId = NetworkId,
                        DisplaySlotId = displaySlotId,
                        ProductId = productID,
                    };

                    //SyncTakeProductFromDisplay(outCustomerTakeProductPacket);

                    Packet packet = new Packet(PacketType.CustomerTakeProductFromDisplay, outCustomerTakeProductPacket);

                    MsmpClient.Instance.SendPayload(packet);
                }
            }
        }

        public void SyncTakeProductFromDisplay(OutCustomerTakeProductPacket packet)
        {
            List<DisplaySlot> displaySlots = Singleton<DisplayManager>.Instance.GetDisplaySlots(packet.ProductId, true);

            if(displaySlots == null || displaySlots.Count == 0)
            {
                return;
            }

            _customer.GetType().GetMethod("TakeProductsFromDisplay", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(_customer, new object[] { displaySlots[0], packet.ProductId });

            StartCoroutine(TakeProductFromDisplay(displaySlots[0], packet.ProductId));
        }

        private IEnumerator TakeProductFromDisplay(DisplaySlot displaySlot, int productId)
        {
            _customer.GetType().GetField("m_TargetDisplay", BindingFlags.NonPublic | BindingFlags.Instance)
                  .SetValue(_customer, displaySlot.Display);

            yield return _customer.StartCoroutine("MoveTo", displaySlot.InteractionPosition);

            CustomerAnimator m_Animator = (CustomerAnimator)_customer.GetType().GetField("m_Animator", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(_customer);

            while (_customer.ShoppingList.Products[productId] > 0 && displaySlot.HasProduct && displaySlot.ProductID == productId)
            {
                bool takeProduct = (bool)_customer.GetType().GetMethod("TakeProduct", BindingFlags.NonPublic | BindingFlags.Instance)
                      .Invoke(_customer, new object[2] { displaySlot, productId });

                if (takeProduct)
                {
                    ItemQuantity m_ShoppingList = (ItemQuantity)_customer.GetType().GetField("m_ShoppingList", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(_customer);

                    m_Animator.PickUpProductFromDisplay(displaySlot.transform.position.y - transform.position.y);

                    _customer.GetType().GetField("m_IsPlayingAnimation", BindingFlags.NonPublic | BindingFlags.Instance)
                        .SetValue(_customer, true);

                    //TODO: fix this reflection bullshit xdd
                    while ((bool)_customer.GetType().GetField("m_IsPlayingAnimation", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_customer))
                    {
                        yield return null;
                    }

                    m_Animator.FinishAnimation(false);

                    if (!m_ShoppingList.Products.ContainsKey(productId))
                    {
                        m_Animator.FinishAnimation(true);
                        _customer.GetType().GetField("m_IsPickingUp", BindingFlags.NonPublic | BindingFlags.Instance)
                            .SetValue(_customer, false);
                        break;
                    }
                }
            }

            ProcessShoppingList();
        }
    }
}
