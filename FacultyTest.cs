using System;
using System.Collections.Generic;
using System.Linq;
using FirstProject.Controllers;
using FirstProject.Data;
using FirstProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using FirstProject.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Debugger.Contracts.HotReload;

namespace UniversityManagementTest;

[TestClass]
public class FacultyCreateTest
{
    // Testing Faculty/Create
    // Faculty can not be created without university
    // This require class init 

    // Database context options
    private static DbContextOptions<ApplicationDbContext>? _options;

    // Default University
    private static University _university = new University()
    {
        UniversityName = "Test1",
        Adress = "TestAdress1",
        CreationDate = DateTime.Now,
        Employed = 60,
        Faculties = new List<Faculty>(2)
    };
    
    [ClassInitialize]
    public static async Task ClassInit(TestContext testContext)
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "FacultyDatabaseTest").Options;
       await using (var context = new ApplicationDbContext(_options))
        {
            await context.AddAsync(_university);
            await context.SaveChangesAsync();
        }
    }

    [TestMethod]
    public async Task Faculty_Successful_Create_Test()
    {
        // Controller should save Faculty in database
        
        // Arrange Section
        await using (var context = new ApplicationDbContext(_options))
        {
            FacultyController facultyController = new FacultyController(context);
            Faculty testFaculty = new Faculty()
            {
                FacultyID = 1,
                FacultyName = "TestFaculty",
                Employed = 30,
                Budget = 1000m,
                CreationDate = DateTime.Now,
                UniversityID = 1
            };
            
            // Act Section
            await facultyController.Create(testFaculty);
            // await context.Faculty.AddAsync(testFaculty);
            // await context.SaveChangesAsync();
            
            // Assert Section
            var facultyQuery = from faculty in context.Faculty select faculty;
            Assert.AreEqual(1, await facultyQuery.CountAsync());
            var fetchedFaculty = await context.Faculty.FirstOrDefaultAsync(f => f.FacultyID == 1);
            Assert.AreNotEqual(null, fetchedFaculty);
            Assert.AreEqual(1, fetchedFaculty.FacultyID);
            Assert.AreEqual(1, fetchedFaculty.UniversityID);
            Assert.AreEqual(30, fetchedFaculty.Employed);
            Assert.AreEqual("TestFaculty", fetchedFaculty.FacultyName);
        }
    }

    [TestMethod]
    public async Task Faculty_Null_University_Create_Test()
    {
        // Controller should return NotFound ViewResult if choosed university does not exists in database
        
        // Arrange Section
        await using (var context = new ApplicationDbContext(_options))
        {
            var controller = new FacultyController(context);
            Faculty testFaculty = new Faculty()
            {
                FacultyID = 1,
                FacultyName = "TestFaculty",
                Employed = 30,
                Budget = 1000m,
                CreationDate = DateTime.Now,
                UniversityID = 4500
            };
            
            // Act Section
            var result  = await controller.Create(testFaculty) as NotFoundResult;
            
            // Assert Section
            Assert.AreNotEqual(null, result);
            Assert.AreEqual(true, result is NotFoundResult);
        }
    }
    [TestCleanup]
    public async Task TestsCleanUp()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            var faculty = await context.Faculty.ToListAsync();
            if (faculty.Count != 0)
            {
                context.RemoveRange(faculty);
                await context.SaveChangesAsync();
            }
        }
    }

    [ClassCleanup]
    public static async Task ClassCleanUp()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            context.Remove(_university);
            await context.SaveChangesAsync();
        }
    }
}

[TestClass]
public class FacultyIndexTest
{
    // Testing data returned from UniversityController which is based on Faculty model

    // Database context options
    private static DbContextOptions<ApplicationDbContext>? _options;

    [ClassInitialize]
    public static async Task ClassTestInit(TestContext testContext)
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "FacultyTestDb").Options;
        await using (var context = new ApplicationDbContext(_options))
        {
            Faculty faculty1 = new Faculty()
            {
                FacultyID = 1,
                FacultyName = "TestFaculty",
                Employed = 30,
                Budget = 1000m,
                CreationDate = DateTime.Now,
                UniversityID = 4500
            };
            Faculty faculty2 = new Faculty()
            {
                FacultyID = 2,
                FacultyName = "TestFaculty2",
                Employed = 30,
                Budget = 1000m,
                CreationDate = DateTime.Now,
                UniversityID = 4500
            };
            await context.Faculty.AddAsync(faculty1);
            await context.Faculty.AddAsync(faculty2);
            await context.University.AddAsync(new University()
            {
                UniversityName = "Test1",
                Adress = "TestAdress1",
                CreationDate = DateTime.Now,
                Employed = 60,
                Faculties = new List<Faculty>(2){faculty1,faculty2}
            });
            await context.SaveChangesAsync();
        }
    }

    [TestMethod]
    public async Task Faculty_Index_Test()
    {
        // Controller should return selected university with faculties attached to them
        
        // Arrange Section
        await using (var context = new ApplicationDbContext(_options))
        {
            var controller = new UniversityController(context);
            
            // Act Section
            var viewResult = await controller.Index("Test1") as ViewResult;
            
            
            // Assert Section
            Assert.AreNotEqual(null,viewResult);
            var resultModel = (University) viewResult.Model;
            Assert.AreEqual(2, resultModel.Faculties.Count);
            var faculty = await context.Faculty.FirstOrDefaultAsync(f => f.FacultyID == 1);
            Assert.AreEqual(true, resultModel.Faculties.Contains(faculty));
        }
    }
    
    [TestMethod]
    public async Task Faculty_Index_NotFound_Test()
    {
        // Controller should return NotFoundResult if university name does not exists in database
        
        // Arrange Section
        await using (var context = new ApplicationDbContext(_options))
        {
            var controller = new UniversityController(context);
            
            // Act Section
            var viewResult = await controller.Index("NotExistingUniversityName") as NotFoundResult;

            // Assert Section
            Assert.AreNotEqual(null,viewResult);
            Assert.AreEqual(true, viewResult is NotFoundResult);
        }
    }
    
    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            var unis = await context.University.ToListAsync();
            var faculties = await context.Faculty.ToListAsync();
            if (unis.Count != 0)
            {
                context.University.RemoveRange(unis);
            }
            if (faculties.Count != 0)
            {
                context.Faculty.RemoveRange(faculties);
            }
            await context.SaveChangesAsync();
        }
    }
}
