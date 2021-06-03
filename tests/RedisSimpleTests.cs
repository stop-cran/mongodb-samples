using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using NUnit.Framework;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using Shouldly;

namespace WebApplication1.Tests
{
    public class RedisSimpleTests
    {
        private CancellationTokenSource _cancel;
        private IConnectionMultiplexer _connectionMultiplexer;
        private IDatabase _db;


        [SetUp]
        public async Task Setup()
        {
            _cancel = new CancellationTokenSource(10000);

            _connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync("localhost");
            _db = _connectionMultiplexer.GetDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            _connectionMultiplexer.Dispose();
            _cancel.Dispose();
        }

        [Test]
        [TestCase("SomeKey", "SomeValue")]
        public async Task ShouldGetSet(string key, string value)
        {
            // Given
            await _db.KeyDeleteAsync(key);

            // When
            var result = await _db.StringSetAsync(key, value);
            var storedValue = await _db.StringGetAsync(key);
            await _db.KeyDeleteAsync(key);

            // Then
            result.ShouldBeTrue();
            storedValue.HasValue.ShouldBeTrue();
            storedValue.ToString().ShouldBe(value);
        }

        [Test]
        [TestCase("SomeKey", "SomeValue")]
        public async Task ShouldAddPopSet(string key, string value)
        {
            // Given
            await _db.KeyDeleteAsync(key);

            // When
            var result1 = await _db.SetAddAsync(key, value + 1);
            var result2 = await _db.SetAddAsync(key, value + 2);
            var result3 = await _db.SetAddAsync(key, value + 3);
            var setMembers3 = await _db.SetMembersAsync(key);
            var randomItem = await _db.SetPopAsync(key);
            var setMembers2 = await _db.SetMembersAsync(key);
            await _db.KeyDeleteAsync(key);

            // Then
            result1.ShouldBeTrue();
            result2.ShouldBeTrue();
            result3.ShouldBeTrue();
            setMembers3.Length.ShouldBe(3);
            randomItem.HasValue.ShouldBeTrue();
            randomItem.ToString().ShouldBeOneOf(value + 1, value + 2, value + 3);
            setMembers2.Length.ShouldBe(2);
        }
        
        
        [Test]
        [TestCase("SomeKey", "SomeValue")]
        public async Task ShouldPushPopList(string key, string value)
        {
            // Given
            await _db.KeyDeleteAsync(key);

            // When
            var result1 = await _db.ListLeftPushAsync(key, value + 1);
            var result2 = await _db.ListRightPushAsync(key, value + 2);
            var listMembers2 = await _db.ListRangeAsync(key);
            var rightItem = await _db.ListRightPopAsync(key);
            await _db.KeyDeleteAsync(key);

            // Then
            result1.ShouldBe(1);
            result2.ShouldBe(2);
            listMembers2.Length.ShouldBe(2);
            rightItem.HasValue.ShouldBeTrue();
            rightItem.ToString().ShouldBe(value + 2);
        }
        
        
        [Test]
        [TestCase("SomeKey", "SomeValue")]
        public async Task ShouldExpire(string key, string value)
        {
            // Given
            await _db.KeyDeleteAsync(key);
            var setResult = await _db.StringSetAsync(key, value);
            var expire = await _db.KeyExpireAsync(key, DateTime.Now.AddMilliseconds(500));
            var getResult1 = await _db.StringGetAsync(key);

            // When
            await Task.Delay(1500);
            
            var getResult2 = await _db.StringGetAsync(key);

            // Then
            setResult.ShouldBeTrue();
            expire.ShouldBeTrue();
            getResult1.HasValue.ShouldBeTrue();
            getResult2.HasValue.ShouldBeFalse();
        }

        [Test]
        [TestCase("SomeKey", "SomeValue")]
        public async Task ShouldLock(string key, string value)
        {
            using var redlockFactory = RedLockFactory.Create(new List<RedLockMultiplexer>
            {
                new(_connectionMultiplexer)
            });

            async Task<bool> ResourceLockFunc()
            {
                using var resourceLock = await redlockFactory.CreateLockAsync(key, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1),_cancel.Token);
                
                bool result = resourceLock.IsAcquired;

                await Task.Delay(3000);

                return result;
            }

            var results = await Task.WhenAll(ResourceLockFunc(), ResourceLockFunc());

            results.Where(b => b).ShouldHaveSingleItem();
        }
    }
}