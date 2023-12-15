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

        public async Task<int> Update(int Id, CourseViewModel request)
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