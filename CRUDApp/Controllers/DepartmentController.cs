using CRUDApp.Data;
using CRUDApp.DataModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRUDApp.Controllers
{

    #region Old Code
    /*[Route("api/[controller]")]
[ApiController]
public class DepartmentController : ControllerBase
{
private readonly OrganizationDbContext mydbcontext;

public DepartmentController(OrganizationDbContext mydbcontext)
{
    this.mydbcontext = mydbcontext;
}

[HttpGet]
public IEnumerable<Department> Get()
{
    return mydbcontext.departments.ToList();
}

[HttpGet("{id}")]
public Department Get(int id)
{
    return mydbcontext.departments.Find(id);
}

[HttpPost]
public Department Post(Department department)
{
    mydbcontext.departments.Add(department);
    mydbcontext.SaveChanges();
    return department;
}

[HttpPut("{id}")]
public string Put(int id, Department department)
{
    mydbcontext.departments.Update(department);
    mydbcontext.SaveChanges();
    return "Record Update Successfully";
}

[HttpDelete("{id}")]

public string Delete(int id)
{
    var departmnt = mydbcontext.departments.Find(id);
    mydbcontext.departments.Remove(departmnt);
    mydbcontext.SaveChanges();
        return "Record Dlete Successfully";
}*/
    #endregion

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "A")]
    public class DepartmentController : ControllerBase
    {
        private readonly OrganizationDbContext mydbcontext;

        public DepartmentController(OrganizationDbContext mydbcontext)
        {
            this.mydbcontext = mydbcontext;
        }

        [HttpGet]
     /*   [Authorize(Roles = "A,U")]*/
        public async Task<ActionResult<IEnumerable<Department>>> Get()
        {
            #region Lambda Expression
            /* var departments = await mydbcontext.departments.Include(x => x.Employees)
                  .Select(x => new Department()
                  {
                      Did = x.Did,
                      DName = x.DName,
                      Description = x.Description,
                      Employees = x.Employees.Select(y => new Employee()
                      {
                          Eid = y.Eid,
                          FullName = y.FirstName + y.LastName,
                          Salary = y.Salary,
                          Department = y.Department
                      })
                  })
                  .ToListAsync();
             return Ok(departments);*/
            #endregion

            try
            {
                var departments = await mydbcontext.departments.Include(x => x.Employees).ToListAsync();

                var jsonDeptsVal = JsonConvert.SerializeObject(departments, Formatting.None, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                return Ok(jsonDeptsVal);
            }
            catch (Exception e)
            {

                return StatusCode(500, "We are Working on Action Level..!");
            }
        }

        [HttpGet("{id}", Name = "Get")]
        public async Task<IActionResult> Get(int id)
        {
            var department = await mydbcontext.departments.FindAsync(id);
            if (department != null)
            {
                return Ok(department);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        /*[Authorize(Roles = "A")]*/
        public async Task<IActionResult> Post(Department department)
        {
            if (User.IsInRole("A"))
            {


                if (ModelState.IsValid)
                {
                    /*// Store name of the user
                    department.CreatedBy = User.Identity.Name;*/

                    // when login to see user id User.Claims first value id is taken
                    department.CreatedBy = User.Claims.First().Value;
                    mydbcontext.departments.Add(department);
                    await mydbcontext.SaveChangesAsync();
                    return CreatedAtAction("Get", new { id = department.Did }, department);
                }
                return BadRequest(ModelState.Values);
            }
            else
            {
                return StatusCode(403);
            }
        }

        [HttpPut("{id}")]
        /*[Authorize(Roles = "A")]*/
        public async Task<IActionResult> Put(int id, Department department)
        {
            if (ModelState.IsValid)
            {
                if (id == department.Did)
                {
                    mydbcontext.departments.Update(department);
                    await mydbcontext.SaveChangesAsync();
                    return NoContent();
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
        [HttpDelete("{id}")]
        /*[Authorize(Roles = "A")]*/
        public async Task<IActionResult> Delete(int id)
        {
            var department = await mydbcontext.departments.FindAsync(id);
            if (department != null)
            {
                mydbcontext.departments.Remove(department);
                mydbcontext.SaveChanges();
                return NoContent();
            }
            return NotFound();
        }
    }
}
