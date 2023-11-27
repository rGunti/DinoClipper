using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PandaDotNet.Tests.Repo.Drivers._TestSetup;

namespace PandaDotNet.Tests.Repo.Drivers
{
    public abstract class BaseRepositoryTest
    {
        protected IContactRepository _repo;

        [TestMethod]
        public virtual void InsertWorks()
        {
            _repo.Insert(new Contact
            {
                Id = "hello-world",
                Name = "John",
                Surname = "Doe"
            });

            Contact helloWorld = _repo["hello-world"];
            Assert.AreEqual("hello-world", helloWorld.Id);
            Assert.AreEqual("John", helloWorld.Name);
            Assert.AreEqual("Doe", helloWorld.Surname);
        }

        protected abstract void SetupDatabaseWithRecords(params Contact[] contacts);

        [TestMethod]
        public virtual void UpdateWorks()
        {
            // Setup
            SetupDatabaseWithRecords(new Contact
            {
                Id = "update-test",
                Name = "John",
                Surname = "Doe"
            });
            
            // Update
            Contact doe = _repo["update-test"];
            doe.Name = "Jane";
            _repo.Update(doe);
            
            // Assert
            Contact jane = _repo["update-test"];
            Assert.AreEqual("Jane", jane.Name);
        }

        [TestMethod]
        public virtual void DeleteWorks()
        {
            // Setup
            SetupDatabaseWithRecords(new Contact
                {
                    Id = "delete-test",
                    Name = "John",
                    Surname = "Doe"
                },
                new Contact
                {
                    Id = "delete-test-2",
                    Name = "Jane",
                    Surname = "Doe"
                });

            Assert.IsTrue(_repo.ExistsWithId("delete-test"));
            Assert.IsTrue(_repo.ExistsWithId("delete-test-2"));

            // Delete
            _repo.Delete("delete-test");

            // Assert
            Assert.IsFalse(_repo.ExistsWithId("delete-test"));
            Assert.IsTrue(_repo.ExistsWithId("delete-test-2"));
            
            // Delete again
            Contact contact = _repo["delete-test-2"];
            _repo.Delete(contact);
            
            // Assert
            Assert.IsFalse(_repo.ExistsWithId("delete-test-2"));
        }
        

        [TestMethod]
        public virtual void ThrowsExceptionOnMissingId()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                Contact _ = _repo["missing-id"];
            });
        }
    }
}