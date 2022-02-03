using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRUDApp.Data;
using CRUDApp.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace CRUDApp.Controllers
{
  
    [Produces("application/xml")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "A,U")]
    public class EmployeesController : ControllerBase
    {
        private readonly OrganizationDbContext _context;

        public EmployeesController(OrganizationDbContext context)
        {
            _context = context;
        }

        // GET: api/Employees
        [HttpGet]
        /*[Authorize(Roles = "A,U")]*/
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployee()
        {
            return await _context.employees.Include(x=>x.Department)
                .Select(y=>new Employee()
                { 
                    Eid=y.Eid,
                   /* FullName=y.FirstName+y.LastName,*/
                    Salary=y.Salary,
                    Department=y.Department
                
                }).ToListAsync();
        }


        // GET: api/Employees/5
        [HttpGet("GetEmployee/{did}")]
       /* [Authorize(Roles ="A,U")]*/
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployee(int did)
        {
            return await _context.employees.Where(a=>a.Did==did).ToListAsync();
        }

        // PUT: api/Employees/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        /*[Authorize(Roles = "A")]*/
        public async Task<IActionResult> PutEmployee(int id, Employee employee)
        {
            if (id != employee.Eid)
            {
                return BadRequest();
            }

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Employees
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        /*[Authorize(Roles = "A")]*/
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            if(ModelState.IsValid)
            {
                _context.employees.Add(employee);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetEmployee", new { id = employee.Eid }, employee);
            }
            else
            {
                return BadRequest(ModelState);
            }
           
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
        /*[Authorize(Roles = "A")]*/
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            _context.employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeeExists(int id)
        {
            return _context.employees.Any(e => e.Eid == id);
        }
    }
}
