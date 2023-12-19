# Part 6: Use dependency injection in .NET

>.NET supports the dependency injection (DI) software design pattern, which is a technique for achieving [Inversion of Control (IoC)](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/architectural-principles#dependency-inversion) between classes and their dependencies. Dependency injection in .NET is a built-in part of the framework, along with configuration, logging, and the options pattern. This guide is compiled based on [Get started with ASP.NET Core MVC](https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/start-mvc?view=aspnetcore-8.0&tabs=visual-studio-code) by `Microsoft`.

In this section:

- Write several interfaces and corresponding implementations
- Use service lifetime and scoping for DI

Before coming to this guide, please refer to [Part 5: Use AutoMapper in MVVM Pattern ASP.NET Core](https://github.com/NguyenPhuDuc307/mvvm-design-pattern).

## Create interfaces and class for services

Create a new interface named `ICoursesService` in the `Services` folder. Replace the generated code with the following:

```c#
using CourseManagement.ViewModels;

namespace CourseManagement.Services
{
    public interface ICoursesService
    {
        Task<IEnumerable<CourseViewModel>> GetAll();
        Task<CourseViewModel> GetById(int id);
        Task<int> Create(CourseRequest request);
        Task<int> Update(CourseViewModel request);
        Task<int> Delete(int id);
    }
}
```

Create a new class named `CoursesService` in the `Services` folder. This class implement `ICoursesService` interface. Replace the generated code with the following:

```c#
using AutoMapper;
using CourseManagement.Data;
using CourseManagement.Data.Entities;
using CourseManagement.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Services
{
    public class CoursesService : ICoursesService
    {
        private readonly CourseDbContext _context;
        private readonly IMapper _mapper;

        public CoursesService(CourseDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<int> Create(CourseRequest request)
        {
            var course = _mapper.Map<Course>(request);
            _context.Add(course);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CourseViewModel>> GetAll()
        {
            var products = await _context.Courses.ToListAsync();
            return _mapper.Map<IEnumerable<CourseViewModel>>(products);
        }

        public async Task<CourseViewModel> GetById(int id)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(m => m.Id == id);
            return _mapper.Map<CourseViewModel>(course);
        }

        public async Task<int> Update(CourseViewModel request)
        {
            if (!CourseExists(request.Id))
            {
                throw new Exception("Course does not exist");
            }
            _context.Update(_mapper.Map<Course>(request));
            return await _context.SaveChangesAsync();
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }
}
```

## Add a service that requires DI

Add the following course service reporter class, which acts as a service to the console app:

Update `CoursesController.cs` with the following code:

```c#
public class CoursesController : Controller
{
    private readonly ICoursesService _coursesService;
    public CoursesController(ICoursesService coursesService)
    {
        _coursesService = coursesService;
    }

    // GET: Courses
    public async Task<IActionResult> Index()
    {
        return View(await _coursesService.GetAll());
    }

    // GET: Courses/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var course = await _coursesService.GetById(id);
        if (course == null)
        {
            return NotFound();
        }
        return View(course);
    }

    // GET: Courses/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Courses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseRequest request)
    {
        if (ModelState.IsValid)
        {
            await _coursesService.Create(request);
            return RedirectToAction(nameof(Index));
        }
        return View(request);
    }

    // GET: Courses/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var course = await _coursesService.GetById(id);
        if (course == null)
        {
            return NotFound();
        }
        return View(course);
    }

    // POST: Courses/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CourseViewModel course)
    {
        if (id != course.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            await _coursesService.Update(course);
            return RedirectToAction(nameof(Index));
        }
        return View(course);
    }

    // GET: Courses/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var course = await _coursesService.GetById(id);
        if (course == null)
        {
            return NotFound();
        }
        return View(course);
    }

    // POST: Courses/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _coursesService.Delete(id);
        return RedirectToAction(nameof(Index));
    }
}
```

## Service lifetimes

Services can be registered with one of the following lifetimes:

- Transient
- Scoped
- Singleton

The following sections describe each of the preceding lifetimes. Choose an appropriate lifetime for each registered service.

### Transient

Transient lifetime services are created each time they're requested from the service container. This lifetime works best for lightweight, stateless services. Register transient services with AddTransient.

In apps that process requests, transient services are disposed at the end of the request.

### Scoped

For web applications, a scoped lifetime indicates that services are created once per client request (connection). Register scoped services with AddScoped.

In apps that process requests, scoped services are disposed at the end of the request.

When using Entity Framework Core, the AddDbContext extension method registers DbContext types with a scoped lifetime by default.

By default, in the development environment, resolving a service from another service with a longer lifetime throws an exception. For more information, see Scope validation.

### Singleton

Singleton lifetime services are created either:

- The first time they're requested.
- By the developer, when providing an implementation instance directly to the container. This approach is rarely needed.

Every subsequent request of the service implementation from the dependency injection container uses the same instance. If the app requires singleton behavior, allow the service container to manage the service's lifetime. Don't implement the singleton design pattern and provide code to dispose of the singleton. Services should never be disposed by code that resolved the service from the container. If a type or factory is registered as a singleton, the container disposes the singleton automatically.

Register singleton services with AddSingleton. Singleton services must be thread safe and are often used in stateless services.

In apps that process requests, singleton services are disposed when the ServiceProvider is disposed on application shutdown. Because memory is not released until the app is shut down, consider memory use with a singleton service.

## Register services for DI

For register services for DI, let's add the following code in `Program.cs`:

```c#
//DI configuration
builder.Services.AddTransient<ICoursesService, CoursesService>();
```

**Final, run the application to test functions:**

Run the following command:

```bash
dotnet watch run
```

Next let's [Part 7: Add search, sorting, pagination to ASP.NET Core MVC application](https://github.com/NguyenPhuDuc307/search-sorting-pagination).
