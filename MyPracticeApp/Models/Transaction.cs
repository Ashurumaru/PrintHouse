using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPracticeApp.Models
{
    public class Transaction
    {
        public DateTime OrderDate { get; set; }
        public string OrderType { get; set; }
        public double Amount { get; set; }
    }
}
