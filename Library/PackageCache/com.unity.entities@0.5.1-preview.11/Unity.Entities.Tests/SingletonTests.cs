﻿using System;
using NUnit.Framework;

namespace Unity.Entities.Tests
{
    class SingletonTests : ECSTestsFixture
    {
        [Test]
        public void GetSetSingleton()
        {
            var entity = m_Manager.CreateEntity(typeof(EcsTestData));

            EmptySystem.SetSingleton(new EcsTestData(10));
            Assert.AreEqual(10, EmptySystem.GetSingleton<EcsTestData>().value);
        }

        [Test]
        public void GetSetSingletonZeroThrows()
        {
            Assert.Throws<InvalidOperationException>(() => EmptySystem.SetSingleton(new EcsTestData()));
            Assert.Throws<InvalidOperationException>(() => EmptySystem.GetSingleton<EcsTestData>());
        }
        
        [Test]
        public void GetSetSingletonMultipleThrows()
        {
            m_Manager.CreateEntity(typeof(EcsTestData));
            m_Manager.CreateEntity(typeof(EcsTestData));

            Assert.Throws<InvalidOperationException>(() => EmptySystem.SetSingleton(new EcsTestData()));
            Assert.Throws<InvalidOperationException>(() => EmptySystem.GetSingleton<EcsTestData>());
        }
        
        [Test]
        public void RequireSingletonWorks()
        {
            EmptySystem.RequireSingletonForUpdate<EcsTestData>();
            EmptySystem.GetEntityQuery(typeof(EcsTestData2));
            
            m_Manager.CreateEntity(typeof(EcsTestData2));
            Assert.IsFalse(EmptySystem.ShouldRunSystem());
            m_Manager.CreateEntity(typeof(EcsTestData));
            Assert.IsTrue(EmptySystem.ShouldRunSystem());
        }

        [AlwaysUpdateSystem]
        class TestAlwaysUpdateSystem : ComponentSystem
        {
            protected override void OnUpdate()
            {
            }
        }

        [Test]
        public void RequireSingletonWithAlwaysUpdateThrows()
        {
            var system = World.CreateSystem<TestAlwaysUpdateSystem>();
            Assert.Throws<InvalidOperationException>(() => system.RequireSingletonForUpdate<EcsTestData>());
        }

        [Test]
        public void HasSingletonWorks()
        {
            Assert.IsFalse(EmptySystem.HasSingleton<EcsTestData>());
            m_Manager.CreateEntity(typeof(EcsTestData));
            Assert.IsTrue(EmptySystem.HasSingleton<EcsTestData>());
        }

        [Test]
        public void HasSingletonThrowsMultiple()
        {
            Assert.IsFalse(EmptySystem.HasSingleton<EcsTestData>());
            m_Manager.CreateEntity(typeof(EcsTestData));
            Assert.IsTrue(EmptySystem.HasSingleton<EcsTestData>());
            m_Manager.CreateEntity(typeof(EcsTestData));
            Assert.IsFalse(EmptySystem.HasSingleton<EcsTestData>());
        }

#if !UNITY_DISABLE_MANAGED_COMPONENTS
        [Test]
        public void GetSetSingleton_ManagedComponents()
        {
            var entity = m_Manager.CreateEntity(typeof(EcsTestManagedComponent));

            const string kTestVal = "SomeString";
            EmptySystem.SetSingleton(new EcsTestManagedComponent() { value = kTestVal });
            Assert.AreEqual(kTestVal, EmptySystem.GetSingleton<EcsTestManagedComponent>().value);
        }

        [Test]
        public void HasSingletonWorks_ManagedComponents()
        {
            Assert.IsFalse(EmptySystem.HasSingleton<EcsTestManagedComponent>());
            m_Manager.CreateEntity(typeof(EcsTestManagedComponent));
            Assert.IsTrue(EmptySystem.HasSingleton<EcsTestManagedComponent>());
        }
#endif
    }
}
