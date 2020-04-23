using System.Collections.Generic;

namespace FirstSample.Models
{
    public class SQLEmployeeRepository : IEmployeeRepository
    {
        public AppDBContext _context { get; }
        public SQLEmployeeRepository(AppDBContext context)
        {
            this._context = context;
        }
        public Employee Add(Employee employee)
        {
           _context.Employees.Add(employee);
           _context.SaveChanges();
           return employee;
        }

        public Employee Delete(int id)
        {
           Employee emp = _context.Employees.Find(id);
           if(emp != null)
           {
               _context.Employees.Remove(emp);
               _context.SaveChanges();
           }
          return emp;
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
           return _context.Employees;
        }

        public Employee GetEmployee(int id)
        {
            return _context.Employees.Find(id);
        }

        public Employee Update(Employee employeeChanges)
        {
            var employee = _context.Employees.Attach(employeeChanges);
            employee.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();
            return employeeChanges;
        }
    }
}