//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Project_BookStoreCT.Models.DataModels
{
    using System;
    using System.Collections.Generic;
    
    public partial class Customer
    {
        public int Customer_ID { get; set; }
        public string customerName { get; set; }
        public string customerEmail { get; set; }
        public string customerPhone { get; set; }
        public string customerAddress { get; set; }
        public Nullable<bool> sex { get; set; }
        public Nullable<System.DateTime> dayOfBirth { get; set; }
        public string password { get; set; }
        public string avatar { get; set; }
    }
}