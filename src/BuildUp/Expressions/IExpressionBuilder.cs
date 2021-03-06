﻿using Stashbox.Entity;
using Stashbox.Registration;
using Stashbox.Resolution;
using System;
using System.Linq.Expressions;

namespace Stashbox.BuildUp.Expressions
{
    internal interface IExpressionBuilder
    {
        Expression CreateFillExpression(IContainerContext containerContext,
            IServiceRegistration serviceRegistration,
            Expression instance,
            ResolutionContext resolutionContext,
            Type serviceType);

        Expression CreateBasicFillExpression(IContainerContext containerContext,
            MemberInformation[] members,
            MethodInformation[] methods,
            Expression instance,
            ResolutionContext resolutionContext,
            Type serviceType);

        Expression CreateExpression(IContainerContext containerContext,
            IServiceRegistration serviceRegistration,
            ResolutionContext resolutionContext,
            Type serviceType);
    }
}
