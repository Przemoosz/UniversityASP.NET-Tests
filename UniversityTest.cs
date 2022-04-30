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

namespace UniversityManagementTest;

[TestClass]
public class UniversitySaveTests
{
    // private University _universityTestObject = new University()
    // {
    //     UniversityName = "Test",
    //     Adress = "TestAdress",
    //     CreationDate = DateTime.MinValue,
    //     Employed = 40,
    //     Faculties = new List<Faculty>(0),
    //     UniversityID = 1
    // };
    
    [TestMethod]
    public async Task University_InMemoryDb_Save_Test()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "FirstProject").Options;
        await using (var context = new ApplicationDbContext(options))
        {
            await context.University.AddAsync(new University()
            {
                UniversityName = "GGG",
                Adress = "Raymonta",
                CreationDate = DateTime.Now,
                Employed = 25,
                Faculties = new List<Faculty>(0),
                UniversityID = 1
            });
            await context.SaveChangesAsync();
        }

        await using (var context = new ApplicationDbContext(options))
        {
            var unis = from uni in context.University select uni;
            
            Assert.AreEqual(1, await unis.CountAsync());
        }

    }

    [TestMethod]
    public async Task University_Create_Test()
    {
        // Arrange Section
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "FisrtProjectDB").Options;
        DateTime date = DateTime.Now;
        
        // Act Section
        await using (var context = new ApplicationDbContext(options))
        {
            var universityController = new UniversityController(context);
            
            University uniTestModel = new University()
            {
                    UniversityName = "GGG",
                    Adress = "Raymonta",
                    CreationDate = date,
                    Employed = 25,
                    Faculties = new List<Faculty>(0),
                    UniversityID = 1
            };
            await universityController.Create(uniTestModel);
        }

        // Assert Section
        await using (var context = new ApplicationDbContext(options))
        {
            var queriedUniveristy = await context.University.Where(i => i.UniversityID == 1).FirstOrDefaultAsync();
            Assert.AreNotEqual(null, queriedUniveristy);
        }
    }

    [TestMethod]
    public async Task University_Create_Data_Test()
    {
        // Arrange Section
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDataBase").Options;
        string testName = "TestName";
        string testAdress = "TestAdress";
        DateTime testDate = DateTime.Now;
        int testEmployedNumber = 40;
        List<Faculty> facultyTestList = new List<Faculty>(0){};
        int testId = 1;

        University university = new University()
        {
            UniversityName = testName,
            Adress = testAdress,
            CreationDate = testDate,
            Employed = testEmployedNumber,
            Faculties = facultyTestList,
            UniversityID = testId
        };
        // Act Section
        await using (var context = new ApplicationDbContext(options))
        {
            UniversityController universityController = new UniversityController(context);
            await universityController.Create(university);
        }
        
        // Assert Section
        await using (var context = new ApplicationDbContext(options))
        {
            University insertedUniversity =
                await context.University.Where(i => i.UniversityID == testId).FirstAsync();
            Assert.AreEqual(testId,insertedUniversity.UniversityID);
            Assert.AreEqual(testName,insertedUniversity.UniversityName);
            Assert.AreEqual(testAdress,insertedUniversity.Adress);
            // No connected faculty so return should be null
            Assert.AreEqual(null,insertedUniversity.Faculties);
            Assert.AreEqual(testDate,insertedUniversity.CreationDate);
        }
        
    }

    [TestMethod]
    public async Task University_Create_NullFaculties_Test()
    {
        // Arrange Section

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabaseNull").Options;
        University universityTestObject = new University()
        {
            UniversityName = "Test",
            Adress = "TestAdress",
            CreationDate = DateTime.MinValue,
            Employed = 40,
            Faculties = null,
            UniversityID = 1
        };
        // Act Section
        await using (var context = new ApplicationDbContext(options))
        {
            var univeristyController = new UniversityController(context);
            await univeristyController.Create(universityTestObject);
        }
        // Assert Section
        await using (var context = new ApplicationDbContext(options))
        {
            University fetchedUni = await context.University.FirstAsync(u => u.UniversityID == 1);
            Assert.AreEqual(null, fetchedUni.Faculties);
        }
    }

    [TestMethod]
    public async Task University_Create_Correct_Return_Test()
    {
        // Arrange Section
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase2").Options;
        
        // Act Section
        await using (var context = new ApplicationDbContext(options))
        {
            University universityTestObject = new University()
            {
                UniversityName = "Test",
                Adress = "TestAdress",
                CreationDate = DateTime.MinValue,
                Employed = 40,
                Faculties = null,
                UniversityID = 1
            };
            
            UniversityController universityController = new UniversityController(context);
            var result  = await universityController.Create(universityTestObject);
            var viewResult = result as RedirectToActionResult;
            
            // Assert Section
            Assert.AreEqual(true, viewResult is RedirectToActionResult);
            Assert.AreEqual("Choose", viewResult.ActionName);
            Assert.AreEqual(null,viewResult.RouteValues);
        }
    }

    [TestMethod]
    public async Task University_Create_SameName_Return_Test()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: "DatabaseSameName")
            .Options;
        await using (var context = new ApplicationDbContext(options))
        {
            var controller = new UniversityController(context);
            University universityTestObject = new University()
            {
                UniversityName = "TestName1",
                Adress = "TestAdress",
                CreationDate = DateTime.MinValue,
                Employed = 40,
                Faculties = null,
                UniversityID = 1
            };

            await controller.Create(universityTestObject);
            universityTestObject.UniversityID = 2;
            var result = await controller.Create(universityTestObject) as RedirectToActionResult;
            Assert.AreEqual("Create", result.ActionName);
            Assert.AreEqual(2, result.RouteValues.Count);
            Assert.AreEqual(true, result.RouteValues["error"]);
            Assert.AreEqual(universityTestObject.UniversityName, result.RouteValues["wrongName"]);
        }
    }
    
    // [TestMethod]
    // public async Task University_Create_Invalid_ModelState_Test()
    // {
    //     var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    //         .UseInMemoryDatabase(databaseName: "TestDatabase2").Options;
    //     await using (var context = new ApplicationDbContext(options))
    //     {
    //         UniversityController universityController = new UniversityController(context);
    //
    //         University university = new University() {UniversityName = "d", Adress = "ddd"};
    //         var result = await universityController.Create(university);
    //
    //         var uni = await context.University.Where(u => u.UniversityID == 1).FirstOrDefaultAsync();
    //         
    //         // Assert.AreEqual("Create", result.ViewName);
    //     }
    // }
}

[TestClass]
public class UniversityChooseAndIndexTest
{
    // Testing Choose and Index Views returned from UniversityController
    
    [TestMethod]
    public async Task University_Choose_Data_Return_Test()
    {
        // Testing University/Choose page
        // Controller should return a list of universities
        
        // Before this tests, all UniversitySaveTests should be passed
        
        // Arrange Section

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase").Options;

        ViewResult response;
        University testUni1 = new University()
        {
            UniversityName = "Test1",
            Adress = "TestAdress1",
            CreationDate = DateTime.Now,
            Employed = 30,
            Faculties = new List<Faculty>(0)
        };
        
        University testUni2 = new University()
        {
            UniversityName = "Test2",
            Adress = "TestAdress2",
            CreationDate = DateTime.Now,
            Employed = 60,
            Faculties = new List<Faculty>(0)
        };
        
        // Act Section
        await using (var context = new ApplicationDbContext(options))
        {
            await context.University.AddAsync(testUni1);
            await context.University.AddAsync(testUni2);
            await context.SaveChangesAsync();
            UniversityController universityController = new UniversityController(context);
            response = await universityController.Choose() as ViewResult;
        }
        
        // Assert Section
        var responseModel = (List<ChooseUniversityModelView>) response.Model;
        Assert.AreEqual(2, responseModel.Count() );
        Assert.AreEqual("Test1", responseModel[0].UniversityName);
        Assert.AreEqual("Test2", responseModel[1].UniversityName);
        Assert.AreEqual(30, responseModel[0].Employed);
        Assert.AreEqual(60, responseModel[1].Employed);
        Assert.AreEqual(0, responseModel[0].FacultiesCount);
        Assert.AreEqual(0, responseModel[1].FacultiesCount);

    }
    
    [TestMethod]
    public async Task University_Index_Not_Found_Test()
    {
        // Testing Index method
        // Controller should return not found when name does not exists in table
        
        // Arrange Section
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "Testdatabase").Options;
        string name = "UniDoesNotExists";
        
        // Act Section
        await using (var context = new ApplicationDbContext(options))
        {
            var controller = new UniversityController(context);
            var result = await controller.Index(name) as NotFoundResult; 
            
            // Assert Section
            Assert.AreEqual(true, result is not null);
        }
        
        

    }

    [TestMethod]
    public async Task University_Index_Test()
    {
        // Controller should return "UniversityView" and model attached to university name
        
        // Arrange Section
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase").Options;
        ViewResult? result;
        University testUni1 = new University()
        {
            UniversityName = "Test1",
            Adress = "TestAdress1",
            CreationDate = DateTime.Now,
            Employed = 30,
            Faculties = new List<Faculty>(0)
        };
        
        // Act section
        await using (var context = new ApplicationDbContext(options))
        {
            var controller = new UniversityController(context);
            await context.University.AddAsync(testUni1);
            await context.SaveChangesAsync();
            result = await controller.Index("Test1") as ViewResult;
        }

        // Assert Section
        Assert.AreEqual("UniversityView", result.ViewName);
        Assert.AreEqual(testUni1,result.Model);
    }
}