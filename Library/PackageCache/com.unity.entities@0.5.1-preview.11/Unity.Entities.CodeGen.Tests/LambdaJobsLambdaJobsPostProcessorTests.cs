using System;
using System.Linq;
using NUnit.Framework;
using Unity.Entities.CodeGen.Tests.LambdaJobs.Infrastructure;
using Unity.Collections;
using Unity.Entities.CodeGen.Tests.TestTypes;
using UnityEngine;

namespace Unity.Entities.CodeGen.Tests
{
    [TestFixture]
    public class LambdaJobsLambdaJobsPostProcessorTests : LambdaJobsPostProcessorTestBase
    {
        [Test]
        public void LambdaTakingUnsupportedArgumentTest()
        {
            AssertProducesError(typeof(LambdaTakingUnsupportedArgument), nameof(UserError.DC0005));
        }

        class LambdaTakingUnsupportedArgument : TestJobComponentSystem
        {
            void Test()
            {
                Entities.ForEach(
                        (string whyAreYouPuttingAStringHereMakesNoSense) => { Console.WriteLine("Hello"); })
                    .Schedule(default);
            }
        }


        [Test]
        public void WithConflictingNameTest()
        {
            AssertProducesError(typeof(WithConflictingName), nameof(UserError.DC0003));
        }

        class WithConflictingName : TestJobComponentSystem
        {
            void Test()
            {
                Entities
                    .WithName("VeryCommonName")
                    .ForEach(
                        (ref Translation t) => { })
                    .Schedule(default);
                
                Entities
                    .WithName("VeryCommonName")
                    .ForEach(
                        (ref Translation t) => { })
                    .Schedule(default);
            }
        }

        [Test]
        public void ConflictingWithNoneTest()
        {
            AssertProducesError(typeof(ConflictingWithNone), nameof(UserError.DC0015), "Translation");
        }

        class ConflictingWithNone : TestJobComponentSystem
        {
            void Test()
            {
                Entities
                    .WithNone<Translation>()
                    .ForEach((in Translation translation) => { })
                    .Schedule(default);
            }
        }

        
        
        [Test]
        public void ConflictingWithNoneBufferElementTest()
        {
            AssertProducesError(typeof(ConflictingWithNoneBufferElement), nameof(UserError.DC0015), "MyBufferFloat");
        }

        class ConflictingWithNoneBufferElement : TestJobComponentSystem
        {
            void Test()
            {
                Entities
                    .WithNone<MyBufferFloat>()
                    .ForEach((in DynamicBuffer<MyBufferFloat> myBuffer) => { })
                    .Run();
            }
        }


        [Test]
        public void ConflictingWithNoneAndWithAnyTest()
        {
            AssertProducesError(typeof(ConflictingWithNoneAndWithAny), nameof(UserError.DC0016), "Translation");
        }

        class ConflictingWithNoneAndWithAny : TestJobComponentSystem
        {
            void Test()
            {
                Entities
                    .WithNone<Translation>()
                    .WithAny<Translation, Velocity>()
                    .ForEach((in Boid translation) => { })
                    .Schedule(default);
            }
        }
        
        
        [Test]
        public void WithNoneWithInvalidTypeTest()
        {
            AssertProducesError(typeof(WithNoneWithInvalidType), nameof(UserError.DC0025), "ANonIComponentDataClass");
        }

        class WithNoneWithInvalidType : TestJobComponentSystem
        {
            class ANonIComponentDataClass
            {
            }
            void Test()
            {
                Entities
                    .WithNone<ANonIComponentDataClass>()
                    .ForEach((in Boid translation) => { })
                    .Schedule(default);
            }
        }

        [Test]
        public void WithReadOnly_IllegalArgument_Test()
        {
            AssertProducesError(typeof(WithReadOnly_IllegalArgument), nameof(UserError.DC0012));
        }


        class WithReadOnly_IllegalArgument : TestJobComponentSystem
        {
            void Test()
            {
                Entities
                    .WithReadOnly("stringLiteral")
                    .ForEach((in Boid translation) => { })
                    .Schedule(default);
            }
        }

        [Test]
        public void WithReadOnly_NonCapturedVariable_Test()
        {
            AssertProducesError(typeof(WithReadOnly_NonCapturedVariable), nameof(UserError.DC0012));
        }

        class WithReadOnly_NonCapturedVariable : TestJobComponentSystem
        {
            void Test()
            {
                var myNativeArray = new NativeArray<float>();

                Entities
                    .WithReadOnly(myNativeArray)
                    .ForEach((in Boid translation) => { })
                    .Schedule(default);
            }
        }

        [Test]
        public void WithUnsupportedParameterTest()
        {
            AssertProducesError(typeof(WithUnsupportedParameter), nameof(UserError.DC0005));
        }

        class WithUnsupportedParameter : TestJobComponentSystem
        {
            void Test()
            {
                Entities
                    .ForEach((string whoKnowsWhatThisMeans, in Boid translation) => { })
                    .Schedule(default);
            }
        }

        [Test]
        public void WithCapturedReferenceTypeTest()
        {
            AssertProducesError(typeof(WithCapturedReferenceType), nameof(UserError.DC0004));
        }

        class WithCapturedReferenceType : TestJobComponentSystem
        {
            class CapturedClass
            {
                public float value;
            }

            void Test()
            {
                var capturedClass = new CapturedClass() {value = 3.0f};
                Entities
                    .ForEach((ref Translation t) => { t.Value = capturedClass.value; })
                    .Schedule(default);
            }
        }
        
        [Test]
        public void NestedScopeWithNonLambdaJobLambdaTest()
        {
            AssertProducesNoError(typeof(NestedScopeWithNonLambdaJobLambda));
        }

        class NestedScopeWithNonLambdaJobLambda : TestJobComponentSystem
        {
            void Test()
            {
                var outerValue = 3.0f;
                {
                    var innerValue = 3.0f;
                    Entities
                        .ForEach((ref Translation t) => { t.Value = outerValue + innerValue; })
                        .Schedule(default);
                }

                DoThing(() => { outerValue = 4.0f; });
            }

            void DoThing(Action action)
            {
                action();
            }
        }

        [Test]
        public void CaptureFieldInLocalCapturingLambdaTest()
        {
            AssertProducesError(typeof(CaptureFieldInLocalCapturingLambda), nameof(UserError.DC0001), "myfield");
        }

        class CaptureFieldInLocalCapturingLambda : TestJobComponentSystem
        {
            private int myfield = 123;

            void Test()
            {
                int also_capture_local = 1;
                Entities
                    .ForEach((ref Translation t) => { t.Value = myfield + also_capture_local; })
                    .Schedule(default);
            }
        }
        
        [Test]
        public void InvokeBaseMethodInBurstLambdaTest()
        {
            AssertProducesError(typeof(InvokeBaseMethodInBurstLambda), nameof(UserError.DC0002), "get_EntityManager");
        }

        class InvokeBaseMethodInBurstLambda : TestJobComponentSystem
        {
            void Test()
            {
                int version = 0;
                Entities.ForEach((ref Translation t) => { version = base.EntityManager.Version; }).Run();
            }
        }

        [Test]
        public void UseSharedComponentData_UsingSchedule_ProducesError()
        {
            AssertProducesError(typeof(SharedComponentDataUsingSchedule), nameof(UserError.DC0019), "MySharedComponentData");
        }

        class SharedComponentDataUsingSchedule : TestJobComponentSystem
        {
            struct MySharedComponentData : ISharedComponentData
            {
            }
            
            void Test()
            {
                Entities
                    .ForEach((MySharedComponentData mydata) => {})
                    .Schedule(default);
            }
        }
        
        [Test]
        public void SharedComponentDataReceivedByRef_ProducesError()
        {
            AssertProducesError(typeof(SharedComponentDataReceivedByRef), nameof(UserError.DC0020), "MySharedComponentData");
        }

        class SharedComponentDataReceivedByRef : TestJobComponentSystem
        {
            struct MySharedComponentData : ISharedComponentData
            {
            }
            
            void Test()
            {
                Entities
                    .WithoutBurst()
                    .ForEach((ref MySharedComponentData mydata) => {})
                    .Run();
            }
        }
        
        [Test]
        public void CustomStructArgumentThatDoesntImplementSupportedInterfaceTest()
        {
            AssertProducesError(typeof(CustomStructArgumentThatDoesntImplementSupportedInterface), nameof(UserError.DC0021), "parameter t has type ForgotToAddInterface. This type is not a");
        }

        class CustomStructArgumentThatDoesntImplementSupportedInterface : TestJobComponentSystem
        {
            struct ForgotToAddInterface
            {
                
            }
            
            void Test()
            {
                Entities
                    .ForEach((ref ForgotToAddInterface t) => { })
                    .Schedule(default);
            }
        }

             
        [Test]
        public void CaptureFromMultipleScopesTest()
        {
            AssertProducesNoError(typeof(CaptureFromMultipleScopes));
        }

        class CaptureFromMultipleScopes : TestJobComponentSystem
        {
            void Test()
            {
                int scope1 = 1;
                {
                    int scope2 = 2;
                    {
                        int scope3 = 3;
                    Entities
                            .ForEach((ref Translation t) => { t.Value = scope1 + scope2 + scope3;})
                        .Schedule(default);
                }
            }
        }
        }

        [Test]
        public void CaptureFieldInNonLocalCapturingLambdaTest()
        {
            AssertProducesError(typeof(CaptureFieldInNonLocalCapturingLambda), nameof(UserError.DC0001), "myfield");
        }

        class CaptureFieldInNonLocalCapturingLambda : TestJobComponentSystem
        {
            private int myfield = 123;

            void Test()
            {
                Entities
                    .ForEach((ref Translation t) => { t.Value = myfield; })
                    .Schedule(default);
            }
        }

        

        [Test]
        public void InvokeInstanceMethodInCapturingLambdaTest()
        {
            AssertProducesError(typeof(InvokeInstanceMethodInCapturingLambda), nameof(UserError.DC0002));
        }

        class InvokeInstanceMethodInCapturingLambda : TestJobComponentSystem
        {
            public object GetSomething(int i) => default;

            void Test()
            {
                int also_capture_local = 1;
                Entities
                    .ForEach((ref Translation t) => { GetSomething(also_capture_local); })
                    .Schedule(default);
            }
        }

        [Test]
        public void InvokeInstanceMethodInNonCapturingLambdaTest()
        {
            AssertProducesError(typeof(InvokeInstanceMethodInNonCapturingLambda), nameof(UserError.DC0002));
        }

        class InvokeInstanceMethodInNonCapturingLambda : TestJobComponentSystem
        {
            public object GetSomething(int i) => default;

            void Test()
            {
                Entities
                    .ForEach((ref Translation t) => { GetSomething(3); })
                    .Schedule(default);
            }
        }



        [Test]
        public void LocalFunctionThatWritesBackToCapturedLocalTest()
        {
            AssertProducesError(typeof(LocalFunctionThatWritesBackToCapturedLocal), nameof(UserError.DC0013));
        }

        class LocalFunctionThatWritesBackToCapturedLocal : TestJobComponentSystem
        {
            void Test()
            {
                int capture_me = 123;
                Entities
                    .ForEach((ref Translation t) =>
                    {
                        void MyLocalFunction()
                        {
                            capture_me++;
                        }

                        MyLocalFunction();
                    }).Schedule(default);
            }
        }

        [Test]
        public void LambdaThatWritesBackToCapturedLocalTest()
        {
            AssertProducesError(typeof(LambdaThatWritesBackToCapturedLocal), nameof(UserError.DC0013));
        }

        class LambdaThatWritesBackToCapturedLocal : TestJobComponentSystem
        {
            void Test()
            {
                int capture_me = 123;
                Entities
                    .ForEach((ref Translation t) => { capture_me++; }).Schedule(default);
            }
        }
        
#if !UNITY_DISABLE_MANAGED_COMPONENTS
        [Test]
        public void ManagedComponentInBurstJobTest()
        {
            AssertProducesError(typeof(ManagedComponentInBurstJob), nameof(UserError.DC0023));
        }

        class ManagedComponent : IComponentData, IEquatable<ManagedComponent>
        {
            public bool Equals(ManagedComponent other) => false;
            public override bool Equals(object obj) => false;
            public override int GetHashCode() =>  0;
        }

        class ManagedComponentInBurstJob : TestJobComponentSystem
        {
           
            void Test()
            {
                Entities.ForEach((ManagedComponent t) => {}).Run();
            }
        }
        
        public void ManagedComponentInScheduleTest()
        {
            AssertProducesError(typeof(ManagedComponentInSchedule), nameof(UserError.DC0023));
        }
        
        class ManagedComponentInSchedule : TestJobComponentSystem
        {
            void Test()
            {
                Entities.ForEach((ManagedComponent t) => {}).Schedule(default);
            }
        }
        
        [Test]
        public void ManagedComponentByReferenceTest()
        {
            AssertProducesError(typeof(ManagedComponentByReference), nameof(UserError.DC0024));
        }

        class ManagedComponentByReference : TestJobComponentSystem
        {
            void Test()
            {
                Entities.WithoutBurst().ForEach((ref ManagedComponent t) => {}).Run();
            }
        }
#endif

        [Test]
        public void WithAllWithSharedFilterTest()
        {
            AssertProducesError(typeof(WithAllWithSharedFilter), nameof(UserError.DC0026), "MySharedComponentData");
        }

        class WithAllWithSharedFilter : TestJobComponentSystem
        {
            struct MySharedComponentData : ISharedComponentData
            {
                public int Value;
            }

            void Test()
            {
                Entities
                    .WithAll<MySharedComponentData>()
                    .WithSharedComponentFilter(new MySharedComponentData() { Value = 3 })
                    .ForEach((in Boid translation) => { })
                    .Schedule(default);
            }
        }

		
        [Test]
        public void WithSwitchStatementTest()
        {
            AssertProducesNoError(typeof(WithSwitchStatement));
        }

        class WithSwitchStatement : TestJobComponentSystem
        {
            struct AbilityControl : IComponentData
            {
                public enum State
                {
                    Idle,
                    Active,
                    Cooldown
                }

                public State behaviorState;
            }

            void Test()
            {
                Entities.WithAll<Translation>()
                    .ForEach((Entity entity, ref AbilityControl abilityCtrl) =>
                    {
                        switch (abilityCtrl.behaviorState)
                        {
                            case AbilityControl.State.Idle:
                                abilityCtrl.behaviorState = AbilityControl.State.Active;
                                break;
                            case AbilityControl.State.Active:
                                abilityCtrl.behaviorState = AbilityControl.State.Cooldown;
                                break;
                            case AbilityControl.State.Cooldown:
                                abilityCtrl.behaviorState = AbilityControl.State.Idle;
                                break;
                        }
                    }).Run();
            }
        }

        [Test]
        public void HasTypesInAnotherAssemblyTest()
        {
            AssertProducesNoError(typeof(HasTypesInAnotherAssembly));
        }

        class HasTypesInAnotherAssembly : TestJobComponentSystem
        {
            void Test()
            {
                Entities
                    .WithAll<BoidInAnotherAssembly>()
                    .WithNone<TranslationInAnotherAssembly>()
                    .WithReadOnly(new VelocityInAnotherAssembly() { Value = 3.0f })
                    .WithAny<AccelerationInAnotherAssembly>()
                    .WithoutBurst()
                    .ForEach((ref RotationInAnotherAssembly a) => {}).Run();
            }
        }
        
        [Test]
        public void LambdaThatMakesNonExplicitStructuralChangesTest()
        {
            AssertProducesError(typeof(LambdaThatMakesNonExplicitStructuralChanges), nameof(UserError.DC0027));
        }

        class LambdaThatMakesNonExplicitStructuralChanges : TestJobComponentSystem
        {
            void Test()
            {
                float delta = 0.0f;
                Entities
                    .WithoutBurst()
                    .ForEach((Entity entity, ref Translation t) =>
                    {
                        float blah = delta + 1.0f;
                        EntityManager.RemoveComponent<Translation>(entity);
                    }).Run();
            }
        }

        [Test]
        public void LambdaThatMakesStructuralChangesWithScheduleTest()
        {
            AssertProducesError(typeof(LambdaThatMakesStructuralChangesWithSchedule), nameof(UserError.DC0028));
        }

        class LambdaThatMakesStructuralChangesWithSchedule : TestJobComponentSystem
        {
            void Test()
            {
                float delta = 0.0f;
                Entities.WithoutBurst()
                    .WithStructuralChanges()
                    .ForEach((Entity entity, ref Translation t) =>
                    {
                        float blah = delta + 1.0f; 
                        EntityManager.RemoveComponent<Translation>(entity); 
                    }).Schedule(default);
            }
        }
        
        [Test]
        public void LambdaThatHasNestedLambdaTest()
        {
            AssertProducesError(typeof(LambdaThatHasNestedLambda), nameof(UserError.DC0029));
        }

        class LambdaThatHasNestedLambda : TestJobComponentSystem
        {
            void Test()
            {
                float delta = 0.0f;
                Entities
                    .WithoutBurst()
                    .ForEach((Entity e1, ref Translation t1) =>
                    {
                        Entities
                            .WithoutBurst()
                            .ForEach((Entity e2, ref Translation t2) => { delta += 1.0f; }).Run();
                    }).Run();
            }
        }
        
        [Test]
        public void LambdaThatTriesToStoreNonValidEntityQueryVariableTest()
        {
            AssertProducesError(typeof(LambdaThatTriesToStoreNonValidEntityQueryVariable), nameof(UserError.DC0031));
        }

        class LambdaThatTriesToStoreNonValidEntityQueryVariable : TestJobComponentSystem
        {
            class EntityQueryHolder
            {
                public EntityQuery m_Query;
            }
            
            void Test()
            {
                EntityQueryHolder entityQueryHolder = new EntityQueryHolder();

                float delta = 0.0f;
                Entities
                    .WithStoreEntityQueryInField(ref entityQueryHolder.m_Query)
                    .ForEach((Entity e2, ref Translation t2) => { delta += 1.0f; }).Run();
            }
        }
        
        [Test]
        public void LambdaThatTriesToStoreLocalEntityQueryVariableTest()
        {
            AssertProducesError(typeof(LambdaThatTriesToStoreLocalEntityQueryVariable), nameof(UserError.DC0031));
        }

        class LambdaThatTriesToStoreLocalEntityQueryVariable : TestJobComponentSystem
        {
            void Test()
            {
                EntityQuery query = null;

                float delta = 0.0f;
                Entities
                    .WithStoreEntityQueryInField(ref query)
                    .ForEach((Entity e2, ref Translation t2) => { delta += 1.0f; }).Run();
            }
        }

#if !NET_DOTS
        [Test]
        public void LambdaInSystemWithExecuteAlwaysTest()
        {
            AssertProducesWarning(typeof(LambdaInSystemWithExecuteAlways), nameof(UserError.DC0032));
        }

        [ExecuteAlways]
        class LambdaInSystemWithExecuteAlways : TestJobComponentSystem
        {
            void Test()
            {
                float delta = 0.0f;
                Entities
                    .ForEach((Entity e1, ref Translation t1) =>
                    {
                        delta += 1.0f;
                    }).Run();
            }
        }
#endif
        
        [Test]
        public void CallsMethodInComponentSystemBaseTest()
        {
            AssertProducesError(typeof(CallsMethodInComponentSystemBase), nameof(UserError.DC0002), "Time");
        }

        class CallsMethodInComponentSystemBase : TestJobComponentSystem
        {
            void Test()
            {
                Entities
                    .ForEach((ref Translation t) => { var targetDistance = Time.DeltaTime; })
                    .Schedule(default);
            }
        }
        
        
        [Test]
        public void IncorrectUsageOfBufferIsDetected()
        {
            AssertProducesError(typeof(IncorrectUsageOfBuffer), nameof(UserError.DC0033), "MyBufferFloat");
        }

        class IncorrectUsageOfBuffer : TestJobComponentSystem
        {
            void Test()
            {
                Entities
                    .ForEach((MyBufferFloat f) => { })
                    .Schedule(default);
            }
        }
        
        [Test]
        public void CorrectUsageOfBufferIsNotDetected()
        {
            AssertProducesNoError(typeof(CorrectUsageOfBuffer));
        }

        class CorrectUsageOfBuffer : TestJobComponentSystem
        {
            void Test()
            {
                Entities
                    .ForEach((DynamicBuffer<MyBufferFloat> f) => { })
                    .Schedule(default);
            }
        }


        [Test]
        public void ParameterNamesHaveStablePath()
        {
            // We expect that the fields for the parameters of a generated job have this path:
            //  {jobName}.Data._lambdaParameterValueProviders.forParameter_{parameterName}._type
            // which we rewrite to
            //  {jobName}.Data.{parameterName}
            // when we retrieve the job data in native code.
            // The important part here is everything following {jobName}.Data.
            var methodToAnalyze = MethodDefinitionForOnlyMethodOf(typeof(ParameterNamesPath));
            var forEachDescriptionConstructions = LambdaJobDescriptionConstruction.FindIn(methodToAnalyze);
            JobStructForLambdaJob jobStructForLambdaJob = LambdaJobsPostProcessor.Rewrite(methodToAnalyze, forEachDescriptionConstructions.First(), null);

            const string valueProviderFieldName = "_lambdaParameterValueProviders";
            var valueProvidersField = jobStructForLambdaJob.TypeDefinition.Fields.FirstOrDefault(f => f.Name == valueProviderFieldName);
            Assert.IsNotNull(valueProvidersField, $"Could not find field {valueProviderFieldName} in generated lambda job!");

            const string parameterFieldName = "forParameter_floatBuffer";
            var parameterField = valueProvidersField.FieldType.Resolve().Fields.FirstOrDefault(f => f.Name == parameterFieldName);
            Assert.IsNotNull(parameterField, $"Could not find field {valueProviderFieldName}.{parameterFieldName} in generated lambda job!");

            const string typeFieldName = "_type";
            var typeField = parameterField.FieldType.Resolve().Fields.FirstOrDefault(f => f.Name == typeFieldName);
            Assert.IsNotNull(typeField, $"Could not find field {valueProviderFieldName}.{parameterFieldName}.{typeFieldName} in generated lambda job!");
        }

        class ParameterNamesPath : TestJobComponentSystem
        {
            void Test()
            {
                Entities
                    .WithName("ParameterNamesTest")
                    .ForEach((DynamicBuffer<MyBufferFloat> floatBuffer) => { })
                    .Schedule(default);
            }
        }

        [Test]
        public void ReadOnlyWarnsAboutArgumentType()
        {
            AssertProducesNoError(typeof(CorrectReadOnlyUsage));
            AssertProducesError(typeof(IncorrectReadOnlyUsage), nameof(UserError.DC0034), "myVar");
        }

        class CorrectReadOnlyUsage : TestJobComponentSystem
        {
            void Test()
            {
                NativeArray<int> array = default;
                Entities
                    .WithReadOnly(array)
                    .ForEach((ref Translation t) => { t.Value += array[0]; })
                    .Schedule(default);
            }
        }

        class IncorrectReadOnlyUsage : TestJobComponentSystem
        {
            void Test()
            {
                int myVar = 0;
                Entities
                    .WithReadOnly(myVar)
                    .ForEach((ref Translation t) => { t.Value += myVar; })
                    .Schedule(default);
            }
        }

        [Test]
        public void DeallocateOnJobCompletionWarnsAboutArgumentType()
        {
            AssertProducesNoError(typeof(CorrectDeallocateOnJobCompletionUsage));
            AssertProducesError(typeof(IncorrectDeallocateOnJobCompletionUsage), nameof(UserError.DC0035), "myVar");
        }

        class CorrectDeallocateOnJobCompletionUsage : TestJobComponentSystem
        {
            void Test()
            {
                NativeArray<int> array = default;
                Entities
                    .WithReadOnly(array)
                    .WithDeallocateOnJobCompletion(array)
                    .ForEach((ref Translation t) => { t.Value += array[0]; })
                    .Schedule(default);
            }
        }

        class IncorrectDeallocateOnJobCompletionUsage : TestJobComponentSystem
        {
            void Test()
            {
                int myVar = 0;
                Entities
                    .WithDeallocateOnJobCompletion(myVar)
                    .ForEach((ref Translation t) => { t.Value += myVar; })
                    .Schedule(default);
            }
        }

        [Test]
        public void DisableContainerSafetyRestrictionWarnsAboutArgumentType()
        {
            AssertProducesNoError(typeof(CorrectDisableContainerSafetyRestrictionUsage));
            AssertProducesError(typeof(IncorrectDisableContainerSafetyRestrictionUsage), nameof(UserError.DC0036), "myVar");
        }

        class CorrectDisableContainerSafetyRestrictionUsage : TestJobComponentSystem
        {
            void Test()
            {
                NativeArray<int> array = default;
                Entities
                    .WithReadOnly(array)
                    .WithNativeDisableContainerSafetyRestriction(array)
                    .ForEach((ref Translation t) => { t.Value += array[0]; })
                    .Schedule(default);
            }
        }

        class IncorrectDisableContainerSafetyRestrictionUsage : TestJobComponentSystem
        {
            void Test()
            {
                int myVar = 0;
                Entities
                    .WithNativeDisableContainerSafetyRestriction(myVar)
                    .ForEach((ref Translation t) => { t.Value += myVar; })
                    .Schedule(default);
            }
        }

        [Test]
        public void DisableParallelForRestrictionWarnsAboutArgumentType()
        {
            AssertProducesNoError(typeof(CorrectDisableParallelForRestrictionUsage));
            AssertProducesError(typeof(IncorrectDisableParallelForRestrictionUsage), nameof(UserError.DC0037), "myVar");
        }

        class CorrectDisableParallelForRestrictionUsage : TestJobComponentSystem
        {
            unsafe void Test()
            {
                NativeArray<int> array = default;
                Entities
                    .WithNativeDisableParallelForRestriction(array)
                    .ForEach((ref Translation t) => { t.Value += array[0]; })
                    .Schedule(default);
            }
        }

        class IncorrectDisableParallelForRestrictionUsage : TestJobComponentSystem
        {
            void Test()
            {
                int myVar = 0;
                Entities
                    .WithNativeDisableParallelForRestriction(myVar)
                    .ForEach((ref Translation t) => { t.Value += myVar; })
                    .Schedule(default);
            }
        }

        [Test]
        public void AttributesErrorWhenUsedOnUserTypeFields()
        {
            AssertProducesError(typeof(CaptureFieldInUserStructLambda), nameof(UserError.DC0038), "UserStruct.Array");
        }

        class CaptureFieldInUserStructLambda : TestJobComponentSystem
        {
            void Test()
            {
                var localStruct = new UserStruct() { Array = default };
                Entities
                    .WithReadOnly(localStruct.Array)
                    .ForEach((ref Translation t) => { t.Value += localStruct.Array[0]; })
                    .Schedule(default);
            }
            struct UserStruct
            {
                public NativeArray<int> Array;
            }
        }
    }
}
