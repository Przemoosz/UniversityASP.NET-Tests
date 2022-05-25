using System;
using System.Collections.Generic;
using System.Linq;
using FirstProject.Controllers;
using FirstProject.Data;
using FirstProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using FirstProject.Models.Enums;
using FirstProject.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Debugger.Contracts.HotReload;
namespace UniversityManagementTest;

[TestClass]
public class CourseCreateTest
{
    // Testing Course/Create
    // Course can not be created without university and faculty
    // This require class init 
    
    private static DbContextOptions<ApplicationDbContext>? _options;
    
    // Default University
    private static University _defaultUniversity = new University()
    {
        UniversityName = "Test1",
        Adress = "TestAdress1",
        CreationDate = DateTime.Now,
        Employed = 60,
    };
    
    // Default Faculty
    private static Faculty _defaultFaculty = new Faculty()
    {
        FacultyID = 1,
        FacultyName = "TestFaculty",
        Employed = 30,
        Budget = 1000m,
        CreationDate = DateTime.Now,
    };

    [ClassInitialize]
    public static async Task ClassTestInit(TestContext testContext)
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: "CourseTests")
            .Options;
        await using (var context = new ApplicationDbContext(_options))
        {
            await context.University.AddAsync(_defaultUniversity);
            await context.Faculty.AddAsync(_defaultFaculty);
            await context.SaveChangesAsync();
            _defaultUniversity.Faculties = new List<Faculty>(1) {_defaultFaculty};
            context.University.Update(_defaultUniversity);
            await context.SaveChangesAsync();
            _defaultFaculty.University = _defaultUniversity;
            _defaultFaculty.UniversityID = 1;
            context.Faculty.Update(_defaultFaculty);
            await context.SaveChangesAsync();
        }
    }

    [TestMethod]
    public async Task Course_Successful_Create_Test()
    {
        // Controller should save Course in database
        
        // Arrange Section
        Course testCourse = new Course()
        {
            CourseName = "TestName",
            CourseType = CourseTypeEnum.BachelorDegree,
            Faculty = _defaultFaculty,
            FacultyID = 1,
            RowVersion = new byte[1],
            Students = new List<StudentModel>(0),
            TotalStudents = 40
        };
        
        // Act Section 
        await using (var context = new ApplicationDbContext(_options!))
        {
            var controller = new CourseController(context);
            var result = await controller.Create(testCourse);
            
            // Assert Section
            var fetchedCourse = await context.Course.FirstOrDefaultAsync(c => c.CourseID == 1);
            Assert.AreNotEqual(null, fetchedCourse);
            Assert.AreEqual("TestName",fetchedCourse!.CourseName);
        }
        
        
    }

    [TestMethod]
    public async Task Course_Faculty_Not_Exists_Create_Test()
    {
        // Controller should return NotFound View after typing facultyId that does not exists
        
        // Arrange Section
        Course testCourse = new Course()
        {
            CourseName = "TestName",
            CourseType = CourseTypeEnum.BachelorDegree,
            FacultyID = 7,
            RowVersion = new byte[1],
            Students = new List<StudentModel>(0),
            TotalStudents = 40
        };
        
        // Act Section
        await using (var context = new ApplicationDbContext(_options!))
        {
            var controller = new CourseController(context);
            var result = await controller.Create(testCourse) as NotFoundResult;
            
            // Assert Section
            Assert.AreNotEqual(null, result);
            Assert.AreEqual(true, result is NotFoundResult);
        }
    }

    [TestMethod]
    public async Task Course_Create_View_Test()
    {
        // Controller should return view with ViewBag.Faculty in it
        
        // Arrange and Act Section
        await using (var context = new ApplicationDbContext(_options!))
        {
            var controller = new CourseController(context);
            var result = controller.Create() as ViewResult;
            
            // Assert Section
            Assert.AreNotEqual(null, result);
            Assert.AreNotEqual(null,result.ViewData["Faculty"]);
            Assert.AreEqual(true, result.ViewData["Faculty"] is SelectList);
            SelectList? faculties = result.ViewData["Faculty"] as SelectList;
            var faculty = from fac in faculties!.Items as IEnumerable<Faculty> where fac.FacultyID ==1 select fac;
            var fetchedFacultyFromSelectList = faculty.First();
            Assert.AreEqual(_defaultFaculty.FacultyName, fetchedFacultyFromSelectList.FacultyName);
        }
    }
    [ClassCleanup]
    public static async Task ClassCleanUp()
    {
        await using (var context = new ApplicationDbContext(_options!))
        {
            context.Remove(_defaultFaculty);
            context.Remove(_defaultUniversity);
            await context.SaveChangesAsync();
        }
    }
    
    
}

[TestClass]
public class CourseTestIndex
{
    // Testing attached View courses which are attached to the same faculty
    // This view is based in FacultyController but its a part of a Course tests

    private static DbContextOptions<ApplicationDbContext>? _options;
    
    // Default University
    private static University _defaultUniversity = new University()
    {
        UniversityName = "Test1",
        Adress = "TestAdress1",
        CreationDate = DateTime.Now,
        Employed = 60,
        // Faculties = new List<Faculty>(1){_defaultFaculty}
    };
    
    // Default Faculty
    private static Faculty _defaultFaculty = new Faculty()
    {
        FacultyID = 1,
        FacultyName = "TestFaculty",
        Employed = 30,
        Budget = 1000m,
        CreationDate = DateTime.Now,
        // UniversityID = 1,
        // University = _defaultUniversity,
        // Courses = new List<Course>(3)
    };
    
    // List of default Courses
    private static List<Course> _defaultCourses = new List<Course>(5)
    {
        new Course()
        {
        CourseName = "DDD",
        CourseType = CourseTypeEnum.MasterDegree,
        Faculty = _defaultFaculty,
        FacultyID = 1,
        RowVersion = new byte[1],
        Students = new List<StudentModel>(0),
        TotalStudents = 10
        },
        new Course()
        {
            CourseName = "CCC",
            CourseType = CourseTypeEnum.EngineerDegree,
            Faculty = _defaultFaculty,
            FacultyID = 1,
            RowVersion = new byte[1],
            Students = new List<StudentModel>(0),
            TotalStudents = 20
        },
        new Course()
        {
            CourseName = "BBB",
            CourseType = CourseTypeEnum.MasterEngineerDegree,
            Faculty = _defaultFaculty,
            FacultyID = 1,
            RowVersion = new byte[1],
            Students = new List<StudentModel>(0),
            TotalStudents = 130
        },
        new Course()
        {
            CourseName = "AAA",
            CourseType = CourseTypeEnum.BachelorDegree,
            Faculty = _defaultFaculty,
            FacultyID = 1,
            RowVersion = new byte[1],
            Students = new List<StudentModel>(0),
            TotalStudents = 4
        },
        new Course()
        {
            CourseName = "EEE",
            CourseType = CourseTypeEnum.MasterDegree,
            Faculty = _defaultFaculty,
            FacultyID = 1,
            RowVersion = new byte[1],
            Students = new List<StudentModel>(0),
            TotalStudents = 10
        },

    };

    [ClassInitialize]
    public static async Task ClassInit(TestContext testContext)
    {
        _options =
            new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: "CourseTestIndex").Options;
        await using (var context = new ApplicationDbContext(_options))
        {
            await context.University.AddAsync(_defaultUniversity);
            await context.Faculty.AddAsync(_defaultFaculty);
            await context.SaveChangesAsync();
            _defaultUniversity.Faculties = new List<Faculty>(1) {_defaultFaculty};
            context.University.Update(_defaultUniversity);
            await context.SaveChangesAsync();
            _defaultFaculty.University = _defaultUniversity;
            _defaultFaculty.UniversityID = 1;
            context.Faculty.Update(_defaultFaculty);
            await context.SaveChangesAsync();
            await context.Course.AddRangeAsync(_defaultCourses);
            await context.SaveChangesAsync();
            _defaultFaculty.Courses = _defaultCourses;
            context.Faculty.Update(_defaultFaculty);
            await context.SaveChangesAsync();
        }
    }

    [TestMethod]
    public async Task Class_Inint_Test()
    {
        // Arrange and Act Section
        await using (var context = new ApplicationDbContext(_options!))
        {
            var faculty = await context.Faculty.Include(c => c.Courses).FirstOrDefaultAsync();
            
            // Assert Section
            Assert.AreNotEqual(null, faculty);
            Assert.AreEqual(5, faculty.Courses.Count);
        }
    }

    [TestMethod]
    public async Task Course_Index_Default_Test()
    {
        // Controller should return 5 course objects 
        
        // Arrange and Act Section
        await using (var context = new ApplicationDbContext(_options!))
        {
            var controller = new FacultyController(context);
            var result = await controller.Courses(1, null,null,null) as ViewResult;
            
            // Assert Section
            Assert.AreNotEqual(null, result);
            var attachedObjects = result!.Model as List<Course>;
            Assert.AreNotEqual(null, attachedObjects);
            Assert.AreEqual(5,attachedObjects!.Count);
        }

    }

    [TestMethod]
    public async Task Course_Index_Search_Test()
    {
        // Controller should return only one object with name "BBB"
        // Arrange and Act Section
        await using (var context = new ApplicationDbContext(_options!))
        {
            var controller = new FacultyController(context);
            var result = await controller.Courses(1, "BBB",null,null) as ViewResult;
            
            // Assert Section
            Assert.AreNotEqual(null, result);
            var attachedObjects = result!.Model as List<Course>;
            Assert.AreNotEqual(null, attachedObjects);
            Assert.AreEqual(1,attachedObjects!.Count);
            Assert.AreEqual("BBB",attachedObjects[0].CourseName);
        }
    }

    [TestMethod]
    public async Task Course_Index_Sort_Name_Ascending_Test()
    {
        // Controller should return all 5 objects but first should have name "AAA"
        // Arrange and Act Section
        await using (var context = new ApplicationDbContext(_options!))
        {
            var controller = new FacultyController(context);
            var result = await controller.Courses(1, null,"Name Ascending",null) as ViewResult;
            
            // Assert Section
            Assert.AreNotEqual(null, result);
            var attachedObjects = result!.Model as List<Course>;
            Assert.AreNotEqual(null, attachedObjects);
            Assert.AreEqual(5,attachedObjects!.Count);
            Assert.AreEqual("AAA",attachedObjects[0].CourseName);
        }
    }
    [TestMethod]
    public async Task Course_Index_Sort_Name_Descending_Test()
    {
        // Controller should return all 5 objects but first should have name "EEE"
        // Arrange and Act Section
        await using (var context = new ApplicationDbContext(_options!))
        {
            var controller = new FacultyController(context);
            var result = await controller.Courses(1, null,"Name Descending",null) as ViewResult;
            
            // Assert Section
            Assert.AreNotEqual(null, result);
            var attachedObjects = result!.Model as List<Course>;
            Assert.AreNotEqual(null, attachedObjects);
            Assert.AreEqual(5,attachedObjects!.Count);
            Assert.AreEqual("EEE",attachedObjects[0].CourseName);
        }
    }
    [TestMethod]
    public async Task Course_Index_Sort_Students_Descending_Test()
    {
        // Controller should return all 5 objects but first should have highest number of students (130) which is course with name "BBB"
        // Arrange and Act Section
        await using (var context = new ApplicationDbContext(_options!))
        {
            var controller = new FacultyController(context);
            var result = await controller.Courses(1, null,"Students Descending",null) as ViewResult;
            
            // Assert Section
            Assert.AreNotEqual(null, result);
            var attachedObjects = result!.Model as List<Course>;
            Assert.AreNotEqual(null, attachedObjects);
            Assert.AreEqual(5,attachedObjects!.Count);
            Assert.AreEqual("BBB",attachedObjects[0].CourseName);
        }
    }
    [TestMethod]
    public async Task Course_Index_Sort_Students_Ascending_Test()
    {
        // Controller should return all 5 objects but first should have highest number of students (4) which is course with name "AAA"
        // Arrange and Act Section
        await using (var context = new ApplicationDbContext(_options!))
        {
            var controller = new FacultyController(context);
            var result = await controller.Courses(1, null,"Students Ascending",null) as ViewResult;
            
            // Assert Section
            Assert.AreNotEqual(null, result);
            var attachedObjects = result!.Model as List<Course>;
            Assert.AreNotEqual(null, attachedObjects);
            Assert.AreEqual(5,attachedObjects!.Count);
            Assert.AreEqual("AAA",attachedObjects[0].CourseName);
        }
    }
    [TestMethod]
    public async Task Course_Index_GroupBy_Test()
    {
        // Controller should return 2 objects with the same CourseType (MasterDegree) also testing sorting by name Descending
        // Arrange and Act Section
        await using (var context = new ApplicationDbContext(_options!))
        {
            var controller = new FacultyController(context);
            var result = await controller.Courses(1, null,"Name Descending",CourseTypeEnum.MasterDegree) as ViewResult;
            
            // Assert Section
            Assert.AreNotEqual(null, result);
            var attachedObjects = result!.Model as List<Course>;
            Assert.AreNotEqual(null, attachedObjects);
            Assert.AreEqual(2,attachedObjects!.Count);
            Assert.AreEqual("EEE",attachedObjects[0].CourseName);
        }
    }
    [TestMethod]
    public async Task Course_Index_Faulty_Not_Exists_Test()
    {
        // Controller should return not found view result after typing not existing faculty id
        // Arrange and Act Section
        await using (var context = new ApplicationDbContext(_options!))
        {
            var controller = new FacultyController(context);
            var result = await controller.Courses(17, null,null, null) as NotFoundResult;
            
            // Assert Section
            Assert.AreNotEqual(null, result);
            Assert.AreEqual(true, result is NotFoundResult);
        }
    }
    [ClassCleanup]
    public static async Task ClassCleanUp()
    {
        await using (var context = new ApplicationDbContext(_options!))
        {
            context.Course.RemoveRange(_defaultCourses);
            context.Faculty.Remove(_defaultFaculty);
            context.University.Remove(_defaultUniversity);
            await context.SaveChangesAsync();
        }
    }
    
}