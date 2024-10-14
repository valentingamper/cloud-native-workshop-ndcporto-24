using System.Text.Json;
using StackExchange.Redis;

namespace Dometrain.Monolith.Api.Courses;

public class CachedCourseRepository : ICourseRepository
{
    private readonly ICourseRepository _courseRepository;
    private readonly IConnectionMultiplexer _multiplexer;

    public CachedCourseRepository(ICourseRepository courseRepository, IConnectionMultiplexer multiplexer)
    {
        _courseRepository = courseRepository;
        _multiplexer = multiplexer;
    }

    public async Task<Course?> CreateAsync(Course course)
    {
        var created = await _courseRepository.CreateAsync(course);
        if (created is null)
        {
            return null;
        }

        var db = _multiplexer.GetDatabase();
        var serializedCourse = JsonSerializer.Serialize(course);
        await db.StringSetAsync($"course_id_{course.Id}", serializedCourse);
        return created;
    }

    public Task<Course?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<Course?> GetBySlugAsync(string slug)
    {
        return _courseRepository.GetBySlugAsync(slug);
    }

    public Task<IEnumerable<Course>> GetAllAsync(string nameFilter, int pageNumber, int pageSize)
    {
        return _courseRepository.GetAllAsync(nameFilter, pageNumber, pageSize);
    }

    public Task<Course?> UpdateAsync(Course course)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}
