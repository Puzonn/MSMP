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
    }
}
