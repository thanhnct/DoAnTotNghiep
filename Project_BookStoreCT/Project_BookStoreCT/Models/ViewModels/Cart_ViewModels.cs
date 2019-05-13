﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_BookStoreCT.Models.ViewModels
{
    public class Cart_ViewModels
    {
        public int book_id { get; set; }
        public string bookname { get; set; }
        public int number { get; set; }
        public double total { get; set; }
        public int ? price { get; set; }
        public string image { get; set; }

    }
}