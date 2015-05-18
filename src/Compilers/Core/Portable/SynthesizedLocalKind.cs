﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis
{
    /// <summary>
    /// Kind of a synthesized local variable.
    /// </summary>
    /// <remarks>
    /// Synthesized local variables are either 
    /// 1) Short-lived (temporary)
    ///    The lifespan of an temporary variable shall not cross a statement boundary (a PDB sequence point).
    ///    These variables are not tracked by EnC and don't have names.
    ///  
    /// 2) Long-lived
    ///    All variables whose lifespan might cross a statement boundary (include a PDB sequence point)
    ///    must be named in a build configuration that supports EnC. Some of them might need to be named in release, to support EE.
    ///    The kind of such local must be encoded in the name, so that we can retrieve it from debug metadata during EnC.
    /// 
    ///    The integer value of the kind must match corresponding Dev11/12 TEMP_KIND enum values for
    ///    compatibility with assemblies generated by the native compiler.
    /// 
    ///    Long-lived local variables must be assigned slots in source order.
    /// </remarks>
    internal enum SynthesizedLocalKind
    {
        /// <summary>
        /// Temp variable created by the optimizer.
        /// </summary>
        OptimizerTemp = -3,

        /// <summary>
        /// Temp variable created during lowering.
        /// </summary>
        LoweringTemp = -2,

        /// <summary>
        /// Temp variable created by the emitter.
        /// </summary>
        EmitterTemp = -1,

        /// <summary>
        /// The variable is not synthesized (C#, VB).
        /// </summary>
        UserDefined = 0,

        /// <summary>
        /// Local variable that stores value of an expression consumed by a subsequent conditional branch instruction that might jump across PDB sequence points.
        /// The value needs to be preserved when remapping the IL offset from old method body to new method body during EnC.
        /// A hidden sequence point also needs to be inserted at the offset where this variable is loaded to be consumed by the branch instruction.
        /// (VB, C#).
        /// </summary>
        ConditionalBranchDiscriminator = 1,

        /// <summary>
        /// Boolean passed to Monitor.Enter (C#, VB).
        /// </summary>
        LockTaken = 2,

        /// <summary>
        /// Variable holding on the object being locked while the execution is within the block of the lock statement (C#) or SyncLock statement (VB).
        /// </summary>
        Lock = 3,

        /// <summary>
        /// Local variable that stores the resources to be disposed at the end of using statement (C#, VB).
        /// </summary>
        Using = 4,

        /// <summary>
        /// Local variable that stores the enumerator instance (C#, VB).
        /// </summary>
        ForEachEnumerator = 5,

        /// <summary>
        /// Local variable that stores the array instance (C#, VB?).
        /// </summary>
        ForEachArray = 6,

        /// <summary>
        /// Local variables that store upper bound of multi-dimentional array, for each dimension (C#, VB?).
        /// </summary>
        ForEachArrayLimit = 7,

        /// <summary>
        /// Local variables that store the current index, for each dimension (C#, VB?).
        /// </summary>
        ForEachArrayIndex = 8,

        /// <summary>
        /// Local variable that holds a pinned handle of a string passed to a fixed statement (C#).
        /// </summary>
        FixedString = 9,

        /// <summary>
        /// Local variable that holds the object passed to With statement (VB). 
        /// </summary>
        With = 10,

        // VB TODO:
        ForLimit = 11,
        // VB TODO:
        ForStep = 12,
        // VB TODO:
        ForInitialValue = 13,
        // VB TODO:
        ForDirection = 14,

        /// <summary>
        /// Local variable used to store the value of Select Case during the execution of Case statements.
        /// </summary>
        SelectCaseValue = 15,

        // VB TODO
        OnErrorActiveHandler = 16,
        // VB TODO
        OnErrorResumeTarget = 17,
        // VB TODO
        OnErrorCurrentStatement = 18,
        // VB TODO
        OnErrorCurrentLine = 19,

        /// <summary>
        /// Local variable that stores the return value of an async method.
        /// </summary>
        AsyncMethodReturnValue = 20,
        StateMachineReturnValue = AsyncMethodReturnValue, // TODO VB: why do we need this in iterators?

        /// <summary>
        /// VB: Stores the return value of a function that is not accessible from user code (e.g. operator, lambda, async, iterator).
        /// C#: Stores the return value of a method/lambda with a block body, so that we can put a sequence point on the closing brace of the body.
        /// </summary>
        FunctionReturnValue = 21,

        TryAwaitPendingException = 22,
        TryAwaitPendingBranch = 23,
        TryAwaitPendingCatch = 24,
        TryAwaitPendingCaughtException = 25,

        /// <summary>
        /// Very special corner case involving filters, await and lambdas.
        /// </summary>
        ExceptionFilterAwaitHoistedExceptionLocal = 26,

        /// <summary>
        /// Local variable that stores the current state of the state machine while MoveNext method is executing.
        /// Used to avoid race conditions due to multiple reads from the lifted state.
        /// </summary>
        StateMachineCachedState = 27,

        /// <summary>
        /// Local that stores an expression value which needs to be spilled.
        /// This local should either be hoisted or its lifespan ends before 
        /// the end of the containing await expression.
        /// </summary>
        AwaitSpill = 28,

        AwaitByRefSpill = 29,

        /// <summary>
        /// Local variable that holds on the display class instance.
        /// </summary>
        LambdaDisplayClass = 30,

        /// <summary>
        /// Local variable used to cache a delegate that is used in inner block (possibly a loop), 
        /// and can be reused for all iterations of the loop.
        /// </summary>
        CachedAnonymousMethodDelegate = 31,

        // VB TODO: XmlInExpressionLambda locals are always lifted and must have distinct names.
        XmlInExpressionLambda = 32,

        /// <summary>
        /// Local variable that stores the result of an await expression (the awaiter object).
        /// The variable is assigned the result of a call to await-expression.GetAwaiter() and subsequently used 
        /// to check whether the task completed. Eventually the value is stored in an awaiter field.
        /// 
        /// The value assigned to the variable needs to be preserved when remapping the IL offset from old method body 
        /// to new method body during EnC. If the awaiter expression is contained in an active statement and the 
        /// containing MoveNext method changes the debugger finds the next sequence point that follows the await expression 
        /// and transfers the execution to the new method version. This sequenec point is placed by the compiler at 
        /// the immediately after the stloc instruction that stores the awaiter object to this variable.
        /// The subsequent ldloc then restores it in the new method version.
        /// 
        /// (VB, C#).
        /// </summary>
        Awaiter = 33,

        /// <summary>
        /// All values have to be less than or equal to <see cref="MaxValidValueForLocalVariableSerializedToDebugInformation"/> 
        /// (<see cref="EditAndContinueMethodDebugInformation"/>)
        /// </summary>
        MaxValidValueForLocalVariableSerializedToDebugInformation = 0x7f - 2,

        /// <summary>
        /// An awaiter in async method. 
        /// Never actually created as a local variable, immediately lifted to a state machine field.
        /// Not serialized to <see cref="EditAndContinueMethodDebugInformation"/>.
        /// </summary>
        AwaiterField = 0x100,

        /// <summary>
        /// The receiver of a delegate relaxation stub.
        /// Created as a local variable but always lifted to a relaxation display class field. 
        /// We never emit debug info for hoisted relaxation variable. 
        /// TODO: Avoid using lambdas and display classes for implementation of relaxation stubs and remove this kind.
        /// </summary>
        DelegateRelaxationReceiver = 0x101,
    }

    internal static class SynthesizedLocalKindExtensions
    {
        public static bool IsLongLived(this SynthesizedLocalKind kind)
        {
            return kind >= SynthesizedLocalKind.UserDefined;
        }

        public static bool MustSurviveStateMachineSuspension(this SynthesizedLocalKind kind)
        {
            // Conditional branch discriminator doens't need to be hoisted. 
            // Its lifetime never spans accross await expression/yield statement.
            // This is true even in cases like:
            // 
            //   if (F(arg, await G())) { ... }
            //
            // Which is emitted as:
            // 
            //   $result = taskAwaiter.GetResult();
            //   $cbd = C.F(sm.spilled_arg, $result);
            //   if ($cbd) { ... }

            return IsLongLived(kind) && kind != SynthesizedLocalKind.ConditionalBranchDiscriminator;
        }

        public static bool IsSlotReusable(this SynthesizedLocalKind kind, OptimizationLevel optimizations)
        {
            if (optimizations == OptimizationLevel.Debug)
            {
                // Don't reuse any long-lived locals in debug builds to provide good debugging experience 
                // for user-defined locals and to allow EnC.
                return !IsLongLived(kind);
            }

            switch (kind)
            {
                // The following variables should always be non-reusable, EE depends on their value.
                case SynthesizedLocalKind.UserDefined:
                case SynthesizedLocalKind.LambdaDisplayClass:
                case SynthesizedLocalKind.With:
                    return false;

                default:
                    return true;
            }
        }

        public static uint PdbAttributes(this SynthesizedLocalKind kind)
        {
            // Marking variables with hidden attribute is only needed for compat with Dev12 EE.
            // We mark all synthesized locals, other than lambda display class as hidden so that they don't whow up in Dev12 EE.
            // Display class is special - it is used by the EE to access variables lifted into a closure.
            return (kind != SynthesizedLocalKind.LambdaDisplayClass && kind != SynthesizedLocalKind.UserDefined && kind != SynthesizedLocalKind.With)
                ? Cci.PdbWriter.HiddenLocalAttributesValue
                : Cci.PdbWriter.DefaultLocalAttributesValue;
        }
    }
}
