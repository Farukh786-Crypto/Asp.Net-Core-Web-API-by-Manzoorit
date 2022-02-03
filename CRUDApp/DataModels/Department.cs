using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CRUDApp.DataModels
{
    [Table("Department")]
    public class Department
    {
        [Key]
        public int Did { get; set; }

        [Required]
        public string DName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public virtual IEnumerable<Employee> Employees { get; set; }
    }
}
