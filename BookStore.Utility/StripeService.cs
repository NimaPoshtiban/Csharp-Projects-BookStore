﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Utility;

public class StripeService
{
    public string SecretKey { get; set; }
    public string PublishableKey { get; set; }
}