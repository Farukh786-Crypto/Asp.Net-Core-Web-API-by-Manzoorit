using CRUDApp.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CRUDApp.DataModels
{

    public class UniqueEmailAttribute:ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value==null)
            {
                return new ValidationResult("Email is requird");
            }
            else
            {
                // when value is not null it has string
                var email = value.ToString();
                // check value to database it is already present or not
                using (OrganizationDbContext dbContext=new OrganizationDbContext())
                {
                    int count = dbContext.employees.Where(a=>a.Email==email).Count();
                    if(count==0)
                    {
                        return ValidationResult.Success;
                    }
                    else
                    {
                        return new ValidationResult("Email is alredy Exist");
                    }
                }
            }
        }
    }

    [Table("Employee")]
    public class Employee
    {
        [Key]
        public int Eid { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [NotMapped]
        /*public string FullName { get; set; }*/
        public string FullName { get { return FirstName + " " + LastName; } }

        [UniqueEmail]
        [EmailAddress]
        public string Email { get; set; }

        [NotMapped]
        [Required,EmailAddress,Compare("Email")]
        public string ConfirmEmail { get; set; }

        [Range(10000,100000)]
        public double Salary { get; set; }

        /*[Url]
        public string LinkedInProfile { get; set; }*/
        public string CreatedBy { get; set; }

        [ForeignKey("Department")]
        public int Did { get; set; }
        public Department Department { get; set; }
    }
}
