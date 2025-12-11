using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Очень_надо
{
    public class Patient
    {
        public string SNILS { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; } // "Мужской" или "Женский"

        public string Diagnosis { get; set; } // новое свойство
    }
}
