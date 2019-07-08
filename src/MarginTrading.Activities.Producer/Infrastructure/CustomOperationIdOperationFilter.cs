// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MarginTrading.Activities.Producer.Infrastructure
{
    [UsedImplicitly]
    public class CustomOperationIdOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var actionDescriptor = (ControllerActionDescriptor) context.ApiDescription.ActionDescriptor;
            operation.OperationId = actionDescriptor.ControllerName + actionDescriptor.ActionName;
        }
    }
}