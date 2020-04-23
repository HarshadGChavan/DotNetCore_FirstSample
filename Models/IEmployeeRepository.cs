using System.Collections.Generic;

namespace FirstSample.Models
{
    public interface IEmployeeRepository
    {
         Employee GetEmployee(int id);
         IEnumerable<Employee> GetAllEmployees();
         Employee Add(Employee employee);
         Employee Update(Employee employee);
         Employee Delete(int id);
    }
}