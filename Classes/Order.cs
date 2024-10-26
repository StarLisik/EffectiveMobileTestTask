using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectiveMobileTestTask.Classes
{
    public class Order  // Класс заказа
    {
        public int Id { get; set; }
        public double Weight { get; set; }
        public string? Area { get; set; }
        public DateTime? OrderTime { get; set; }

        public Order(int id, double weight, string? area, DateTime? orderTime)
        {
            Id = id;
            Weight = weight;
            Area = area;
            OrderTime = orderTime;
        }
    }
}
