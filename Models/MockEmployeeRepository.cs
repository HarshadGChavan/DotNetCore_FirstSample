using System.Collections.Generic;
using System.Linq;

namespace FirstSample.Models
{
    public class MockEmployeeRepository : IEmployeeRepository
    {
        private List<Employee> _employeeList;
        public MockEmployeeRepository()
        {
            _employeeList = new List<Employee>()
            {
                new Employee() {Id=1,Name="Mary",Department=Dept.HR,Email="Mary@gmail.com"},
                new Employee() {Id=2,Name="John",Department=Dept.IT,Email="John@gmail.com"},
                new Employee() {Id=3,Name="Sam",Department=Dept.IT,Email="Sam@gmail.com"}
            };
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return _employeeList;
        }

        public Employee GetEmployee(int id)
        {
            Employee e = _employeeList.FirstOrDefault(e=>e.Id== id);
            return e;
        }

        public Employee Add(Employee employee)
        {
             employee.Id=_employeeList.Max(employee=>employee.Id) + 1;
            _employeeList.Add(employee);
            return employee;
        }

        public Employee Update(Employee employee)
        {
           Employee emp = _employeeList.FirstOrDefault(e =>e.Id == employee.Id);
           if(emp!=null)
           {
              emp.Name= employee.Name;
              emp.Email = employee.Email;
              emp.Department = employee.Department;
           }
           return emp;
        }

        public Employee Delete(int id)
        {
           Employee emp =  _employeeList.FirstOrDefault(e=>e.Id == id);
           if(emp!=null)
           {
               _employeeList.Remove(emp);
           }
           return emp;
        }
    }
}