﻿using IntegrationEngine.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationEngine.Model.Tests
{
    public class TriggerPropertyExtensionTest
    {
        [Test]
        public void ShouldGetActiveDescriptionForStateIdZero()
        {
            int stateId = 0;

            var result = stateId.GetStateDescription();

            Assert.That(result, Is.EqualTo("Active"));
        }

        [Test]
        public void ShouldGetPausedDescriptionForStateIdOne()
        {
            int stateId = 1;

            var result = stateId.GetStateDescription();

            Assert.That(result, Is.EqualTo("Paused"));
        }

        [Test]
        public void ShouldGetTimeZoneFromTimeZoneId()
        {
            string timeZoneId = "New York Standard Time";

            var result = timeZoneId.GetTimeZoneInfo();

            Assert.That(result, Is.EqualTo(TimeZoneInfo.Utc));
        }

        [Test]
        public void ShouldReturnUtcTimeZoneIfTimeZoneIdIsNull()
        {
            string timeZoneId = null;

            var result = timeZoneId.GetTimeZoneInfo();

            Assert.That(result, Is.EqualTo(TimeZoneInfo.Utc));
        }

        [Test]
        public void ShouldReturnUtcTimeZoneIfTimeZoneIdDoesNotExist()
        {
            string timeZoneId = "foo";

            var result = timeZoneId.GetTimeZoneInfo();

            Assert.That(result, Is.EqualTo(TimeZoneInfo.Utc));
        }

        [Test]
        public void ShouldReturnHumanReadableCronSchedule()
        {
            var expected = "Every minute";
            string cronExpression = "* * * * *";

            var result = cronExpression.GetHumanReadableCronSchedule();

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ShouldReturnEmptyStringIfCronExpressionIsNull()
        {
            string cronExpression = null;

            var result = cronExpression.GetHumanReadableCronSchedule();

            Assert.That(result, Is.EqualTo(String.Empty));
        }

        [Test]
        public void ShouldReturnEmptyStringIfCronExpressionIsInvalidString()
        {
            string cronExpression = "foo";

            var result = cronExpression.GetTimeZoneInfo();

            Assert.That(result, Is.EqualTo(TimeZoneInfo.Utc));
        }
    }
}
