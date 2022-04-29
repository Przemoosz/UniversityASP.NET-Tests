using System;
using System.Collections.Generic;
using System.Linq;
using FirstProject.Controllers;
using FirstProject.Data;
using FirstProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
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
public class UniversityChooseTest
{
    
}