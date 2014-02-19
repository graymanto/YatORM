﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

using YatORM.Tests.Entity;
using YatORM.Tests.Settings;
using YatORM.Tests.TestTools;

namespace YatORM.Tests
{
    [TestFixture]
    public class FindEntityTests
    {
        private IDataSession _session;

        [SetUp]
        public void Setup()
        {
            _session = SessionBuilder.WithConnectionString(TestSettings.ConnectionString).BuildSession();

            CommandRunner.ClearEntityTable<SingleStringTestTable>();
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
        public void Find_ByIdAndStringField_ExpectFindsCorrectEntity()
        {
            var testIdValue = Guid.NewGuid();
            var testStringValue = Guid.NewGuid().ToString();

            var testEntity = new SingleStringTestTable { Id = testIdValue, TestString = testStringValue };
            CommandRunner.InsertEntity(testEntity);

            var entity = _session.Find<SingleStringTestTable>(s => s.Id == testIdValue && s.TestString == testStringValue);

            entity.Should().NotBeNull();
            entity.Id.Should().Be(testIdValue);
            entity.TestString.Should().Be(testStringValue);
        }
    }
}
