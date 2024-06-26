﻿using System;
using UnityEngine;
using VRC.SDK3.Dynamics.Contact.Components;

namespace Surge.Models
{
    [Serializable]
    internal class ContactInfo
    {
        [field: SerializeField]
        public VRCContactReceiver? ContactReceiver { get; private set; }
    }
}