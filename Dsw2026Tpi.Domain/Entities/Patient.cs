using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Patient : EntityBase
    {
        public string Name { get; init; }
        public string PhoneNumber { get; init; }
        public string Dni { get; private set; }
        public bool Deleted { get; private set; } = false;

        #region Constructor for EF
#pragma warning disable CS8618
        private Patient()
        {
        }
#pragma warning restore CS8618
        #endregion

        public Patient (string name, string phonenumber, string dni) : base()
        {
            Name = name;
            PhoneNumber = phonenumber;
            Dni = dni;
            CreatedAt = DateTime.Now;

        }

        public void Deactivate()
        {
            Deleted = true;
        }
    }

}
