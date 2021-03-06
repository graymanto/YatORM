﻿using System;

using FluentAssertions;

using NUnit.Framework;

using YatORM.Tests.Attributes;
using YatORM.Tests.Entity;
using YatORM.Tests.Settings;
using YatORM.Tests.TestTools;

namespace YatORM.Tests
{
    [TestFixture, Rollback]
    public class FindEntityTests
    {
        private IDataSession _session;

        [SetUp]
        public void Setup()
        {
            _session = SessionBuilder.WithConnectionString(TestSettings.ConnectionString).BuildSession();

            CommandRunner.ClearEntityTable<SingleStringTestTable>();
            CommandRunner.ClearEntityTable<TypeTestTable>();
        }

        [Test]
        public void Find_NothingInTable_ExpectReturnsNull()
        {
            var anyGuid = Guid.NewGuid();

            var isNull = _session.Find<SingleStringTestTable>(s => s.Id == anyGuid);

            isNull.Should().BeNull();
        }

        [Test]
        public void Find_WrongEntryInTable_ExpectReturnsNull()
        {
            var testEntity = new SingleStringTestTable { Id = Guid.NewGuid(), TestString = "Any string" };
            CommandRunner.InsertEntity(testEntity);

            var anyGuid = Guid.NewGuid();
            var isNull = _session.Find<SingleStringTestTable>(s => s.Id == anyGuid);

            isNull.Should().BeNull();
        }

        [Test]
        public void Find_ByIdOnly_ExpectFindsCorrectEntity()
        {
            var testIdValue = Guid.NewGuid();
            var testStringValue = Guid.NewGuid().ToString();

            var testEntity = new SingleStringTestTable { Id = testIdValue, TestString = testStringValue };
            CommandRunner.InsertEntity(testEntity);

            var entity = _session.Find<SingleStringTestTable>(s => s.Id == testIdValue);

            entity.Should().NotBeNull();
            entity.Id.Should().Be(testIdValue);
            entity.TestString.Should().Be(testStringValue);
        }

        [Test]
        public void Find_ByIdWithComplexExpression_ExpectFindsCorrectEntity()
        {
            var testIdValue = Guid.NewGuid();
            var testStringValue = Guid.NewGuid().ToString();

            var testEntity = new SingleStringTestTable { Id = testIdValue, TestString = testStringValue };
            CommandRunner.InsertEntity(testEntity);

            var entity = _session.Find<SingleStringTestTable>(s => s.Id == testEntity.Id);

            entity.Should().NotBeNull();
            entity.Id.Should().Be(testIdValue);
            entity.TestString.Should().Be(testStringValue);
        }

        [Test]
        public void Find_ByIdOnlyUsingConstant_ExpectFindsCorrectEntity()
        {
            var testIdValue = Guid.NewGuid();

            var testEntity = new SingleStringTestTable { Id = testIdValue, TestString = "12345" };
            CommandRunner.InsertEntity(testEntity);

            var entity = _session.Find<SingleStringTestTable>(s => s.TestString == "12345");

            entity.Should().NotBeNull();
            entity.Id.Should().Be(testIdValue);
            entity.TestString.Should().Be("12345");
        }

        [Test]
        public void Find_ByIdAndStringField_ExpectFindsCorrectEntity()
        {
            var testIdValue = Guid.NewGuid();
            var testStringValue = Guid.NewGuid().ToString();

            var testEntity = new SingleStringTestTable { Id = testIdValue, TestString = testStringValue };
            CommandRunner.InsertEntity(testEntity);

            var entity =
                _session.Find<SingleStringTestTable>(s => s.Id == testIdValue && s.TestString == testStringValue);

            entity.Should().NotBeNull();
            entity.Id.Should().Be(testIdValue);
            entity.TestString.Should().Be(testStringValue);
        }

        [Test]
        public void Find_ByIdOrStringField_ExpectFindsCorrectEntity()
        {
            var testIdValue = Guid.NewGuid();
            var testStringValue = Guid.NewGuid().ToString();

            var testEntity = new SingleStringTestTable { Id = testIdValue, TestString = testStringValue };
            CommandRunner.InsertEntity(testEntity);

            var entity =
                _session.Find<SingleStringTestTable>(s => s.Id == testIdValue || s.TestString == testStringValue);

            entity.Should().NotBeNull();
            entity.Id.Should().Be(testIdValue);
            entity.TestString.Should().Be(testStringValue);
        }

        [Test]
        public void Find_ByIntField_ExpectFindsEntity()
        {
            var testEntity = CreateTypeTestEntity();
            CommandRunner.InsertEntity(testEntity);

            int testInt = testEntity.TestInt;

            var entity = _session.Find<TypeTestTable>(t => t.TestInt == testInt);

            entity.Should().NotBeNull();
            entity.TestInt.Should().Be(testEntity.TestInt);
            entity.Id.Should().Be(testEntity.Id);
        }

        [Test]
        public void Find_ByBigIntField_ExpectFindsEntity()
        {
            var testEntity = CreateTypeTestEntity();
            CommandRunner.InsertEntity(testEntity);

            long testBigInt = testEntity.TestBigInt;

            var entity = _session.Find<TypeTestTable>(t => t.TestBigInt == testBigInt);

            entity.Should().NotBeNull();
            entity.TestBigInt.Should().Be(testEntity.TestBigInt);
            entity.Id.Should().Be(testEntity.Id);
        }

        [Test]
        public void Find_ByDateTimeField_ExpectFindsEntity()
        {
            var testEntity = CreateTypeTestEntity();
            CommandRunner.InsertEntity(testEntity);

            var queryField = testEntity.TestDate;

            var entity = _session.Find<TypeTestTable>(t => t.TestDate == queryField);

            entity.Should().NotBeNull();
            entity.TestDate.Should().Be(testEntity.TestDate);
            entity.Id.Should().Be(testEntity.Id);
        }

        [Test]
        public void Find_ByIntGreaterThan_ExpectFindsEntity()
        {
            var testEntity = CreateTypeTestEntity();
            CommandRunner.InsertEntity(testEntity);

            const int QueryField = 1;

            var entity = _session.Find<TypeTestTable>(t => t.TestInt > QueryField);

            entity.Should().NotBeNull();
            entity.Id.Should().Be(testEntity.Id);
        }

        [Test]
        public void Find_ByIntGreaterEquals_ExpectFindsEntity()
        {
            var testEntity = CreateTypeTestEntity();
            CommandRunner.InsertEntity(testEntity);

            var queryField = testEntity.TestInt;

            var entity = _session.Find<TypeTestTable>(t => t.TestInt >= queryField);

            entity.Should().NotBeNull();
            entity.Id.Should().Be(testEntity.Id);
        }

        [Test]
        public void Find_ByIntGreaterThanWithNoMatch_ExpectReturnsNull()
        {
            var testEntity = CreateTypeTestEntity();
            CommandRunner.InsertEntity(testEntity);

            const int QueryField = 10;

            var entity = _session.Find<TypeTestTable>(t => t.TestInt > QueryField);

            entity.Should().BeNull();
        }

        [Test]
        public void Find_ByIntLessThan_ExpectFindsEntity()
        {
            var testEntity = CreateTypeTestEntity();
            CommandRunner.InsertEntity(testEntity);

            const int QueryField = 10;

            var entity = _session.Find<TypeTestTable>(t => t.TestInt < QueryField);

            entity.Should().NotBeNull();
            entity.Id.Should().Be(testEntity.Id);
        }

        [Test]
        public void Find_ByIntLessThanEquals_ExpectFindsEntity()
        {
            var testEntity = CreateTypeTestEntity();
            CommandRunner.InsertEntity(testEntity);

            var queryField = testEntity.TestInt;

            var entity = _session.Find<TypeTestTable>(t => t.TestInt <= queryField);

            entity.Should().NotBeNull();
            entity.Id.Should().Be(testEntity.Id);
        }

        [Test]
        public void Find_ByIntLessThanWithNoMatch_ExpectReturnsNull()
        {
            var testEntity = CreateTypeTestEntity();
            CommandRunner.InsertEntity(testEntity);

            const int QueryField = 0;

            var entity = _session.Find<TypeTestTable>(t => t.TestInt < QueryField);

            entity.Should().BeNull();
        }

        private TypeTestTable CreateTypeTestEntity()
        {
            return new TypeTestTable
                       {
                           Id = Guid.NewGuid(),
                           TestBigInt = 5,
                           TestDate = new DateTime(2014, 1, 1),
                           TestInt = 2,
                           TestNullBigInt = 4,
                           TestNullDate = new DateTime(2014, 1, 2),
                           TestNullInt = 7,
                           TestString = "78910"
                       };
        }
    }
}