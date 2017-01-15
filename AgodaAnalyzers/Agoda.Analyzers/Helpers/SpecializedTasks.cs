// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace Agoda.Analyzers.Helpers
{
    public static class SpecializedTasks
    {
        public static Task CompletedTask { get; } = Task.FromResult(default(VoidResult));

        private struct VoidResult
        {
        }
    }
}
