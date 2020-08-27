﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.PropertyMocks
{
    /// <summary>
    /// Middleware which injests mocked settings properties.
    /// </summary>
    public class MockSettingsMiddleware : IMiddleware
    {
        private readonly string prefix = $"{ScopePath.Settings}.";
        private readonly Dictionary<string, string> mockData = new Dictionary<string, string>();
        private bool configured = false;
        private IConfiguration configuredConfiguration = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockSettingsMiddleware"/> class.
        /// </summary>
        /// <param name="properties">properties to mock.</param>
        public MockSettingsMiddleware(List<PropertyMock> properties)
        {
            foreach (var property in properties)
            {
                if (property is PropertiesMock mock)
                {
                    foreach (var assignment in mock.Assignments)
                    {
                        if (assignment.Property.StartsWith(prefix, StringComparison.Ordinal))
                        {
                            if (assignment.Value is string value)
                            {
                                var path = assignment.Property.Substring(prefix.Length);

                                if (!mockData.ContainsKey(path))
                                {
                                    // Note that settings use : as separator in ConfigurationBuilder.
                                    mockData.Add(path.Replace('.', ':'), value);
                                }
                            }
                            else
                            {
                                throw new NotSupportedException($"Only string is supported as value for mocking settings. {assignment.Value} is not supported.");
                            }
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            if (!configured)
            {
                if (mockData.Count != 0)
                {
                    IConfigurationBuilder configBuilder = new ConfigurationBuilder();

                    // We assume bot will use TurnState to store settings' configuration.
                    var previousConfig = turnContext.TurnState.Get<IConfiguration>();
                    if (previousConfig != null)
                    {
                        configBuilder = configBuilder.AddConfiguration(previousConfig);
                    }

                    configBuilder.AddInMemoryCollection(mockData);
                    configuredConfiguration = configBuilder.Build();
                    mockData.Clear();
                }

                configured = true;
            }

            if (configuredConfiguration != null)
            {
                turnContext.TurnState.Set<IConfiguration>(configuredConfiguration);
            }

            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
