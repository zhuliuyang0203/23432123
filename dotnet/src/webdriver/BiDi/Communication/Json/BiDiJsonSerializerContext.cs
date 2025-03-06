// <copyright file="BiDiJsonSerializerContext.cs" company="Selenium Committers">
// Licensed to the Software Freedom Conservancy (SFC) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The SFC licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenQA.Selenium.BiDi.Communication.Json;

#region https://github.com/dotnet/runtime/issues/72604
[JsonSerializable(typeof(MessageSuccess))]
[JsonSerializable(typeof(MessageError))]
[JsonSerializable(typeof(MessageEvent))]

[JsonSerializable(typeof(Modules.Script.EvaluateResult.Success))]
[JsonSerializable(typeof(Modules.Script.EvaluateResult.Exception))]

[JsonSerializable(typeof(Modules.Script.RemoteValue.Number), TypeInfoPropertyName = "Script_RemoteValue_Number")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Boolean), TypeInfoPropertyName = "Script_RemoteValue_Boolean")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.String), TypeInfoPropertyName = "Script_RemoteValue_String")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Null), TypeInfoPropertyName = "Script_RemoteValue_Null")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Undefined), TypeInfoPropertyName = "Script_RemoteValue_Undefined")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Symbol))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Array), TypeInfoPropertyName = "Script_RemoteValue_Array")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Object), TypeInfoPropertyName = "Script_RemoteValue_Object")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Function))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.RegExp), TypeInfoPropertyName = "Script_RemoteValue_RegExp")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.RegExp.RegExpValue), TypeInfoPropertyName = "Script_RemoteValue_RegExp_RegExpValue")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Date), TypeInfoPropertyName = "Script_RemoteValue_Date")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Map), TypeInfoPropertyName = "Script_RemoteValue_Map")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Set), TypeInfoPropertyName = "Script_RemoteValue_Set")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.WeakMap))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.WeakSet))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Generator))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Error))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Proxy))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Promise))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.TypedArray))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.ArrayBuffer))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.NodeList))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.HtmlCollection))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Node))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.WindowProxy))]

[JsonSerializable(typeof(Modules.Script.RealmInfo.Window))]
[JsonSerializable(typeof(Modules.Script.RealmInfo.DedicatedWorker))]
[JsonSerializable(typeof(Modules.Script.RealmInfo.SharedWorker))]
[JsonSerializable(typeof(Modules.Script.RealmInfo.ServiceWorker))]
[JsonSerializable(typeof(Modules.Script.RealmInfo.Worker))]
[JsonSerializable(typeof(Modules.Script.RealmInfo.PaintWorklet))]
[JsonSerializable(typeof(Modules.Script.RealmInfo.AudioWorklet))]
[JsonSerializable(typeof(Modules.Script.RealmInfo.Worklet))]

[JsonSerializable(typeof(Modules.Log.Entry.Console))]
[JsonSerializable(typeof(Modules.Log.Entry.Javascript))]
#endregion

[JsonSerializable(typeof(Command))]
[JsonSerializable(typeof(Message))]

[JsonSerializable(typeof(Modules.Session.StatusCommand))]
[JsonSerializable(typeof(Modules.Session.StatusResult))]
[JsonSerializable(typeof(Modules.Session.NewCommand))]
[JsonSerializable(typeof(Modules.Session.NewResult))]
[JsonSerializable(typeof(Modules.Session.EndCommand))]
[JsonSerializable(typeof(Modules.Session.SubscribeCommand))]
[JsonSerializable(typeof(Modules.Session.SubscribeResult))]
[JsonSerializable(typeof(Modules.Session.UnsubscribeByIdCommand))]
[JsonSerializable(typeof(Modules.Session.UnsubscribeByAttributesCommand))]

[JsonSerializable(typeof(Modules.Browser.CloseCommand), TypeInfoPropertyName = "Browser_CloseCommand")]
[JsonSerializable(typeof(Modules.Browser.CreateUserContextCommand))]
[JsonSerializable(typeof(Modules.Browser.GetUserContextsCommand))]
[JsonSerializable(typeof(Modules.Browser.GetUserContextsResult))]
[JsonSerializable(typeof(Modules.Browser.RemoveUserContextCommand))]
[JsonSerializable(typeof(Modules.Browser.GetClientWindowsCommand))]
[JsonSerializable(typeof(Modules.Browser.GetClientWindowsResult))]
[JsonSerializable(typeof(Modules.Browser.UserContextInfo))]
[JsonSerializable(typeof(IReadOnlyList<Modules.Browser.UserContextInfo>))]
[JsonSerializable(typeof(IReadOnlyList<Modules.Browser.ClientWindowInfo>))]


[JsonSerializable(typeof(Modules.BrowsingContext.ActivateCommand))]
[JsonSerializable(typeof(Modules.BrowsingContext.CaptureScreenshotCommand))]
[JsonSerializable(typeof(Modules.BrowsingContext.CaptureScreenshotResult))]
[JsonSerializable(typeof(Modules.BrowsingContext.CloseCommand), TypeInfoPropertyName = "BrowsingContext_CloseCommand")]
[JsonSerializable(typeof(Modules.BrowsingContext.CreateCommand))]
[JsonSerializable(typeof(Modules.BrowsingContext.CreateResult))]
[JsonSerializable(typeof(Modules.BrowsingContext.GetTreeCommand))]
[JsonSerializable(typeof(Modules.BrowsingContext.GetTreeResult))]
[JsonSerializable(typeof(Modules.BrowsingContext.HandleUserPromptCommand))]
[JsonSerializable(typeof(Modules.BrowsingContext.LocateNodesCommand))]
[JsonSerializable(typeof(Modules.BrowsingContext.LocateNodesResult))]
[JsonSerializable(typeof(Modules.BrowsingContext.NavigateCommand))]
[JsonSerializable(typeof(Modules.BrowsingContext.NavigateResult))]
[JsonSerializable(typeof(Modules.BrowsingContext.PrintCommand))]
[JsonSerializable(typeof(Modules.BrowsingContext.PrintResult))]
[JsonSerializable(typeof(Modules.BrowsingContext.ReloadCommand))]
[JsonSerializable(typeof(Modules.BrowsingContext.SetViewportCommand))]
[JsonSerializable(typeof(Modules.BrowsingContext.TraverseHistoryCommand))]
[JsonSerializable(typeof(Modules.BrowsingContext.TraverseHistoryResult))]
[JsonSerializable(typeof(Modules.BrowsingContext.BrowsingContextInfo))]
[JsonSerializable(typeof(Modules.BrowsingContext.NavigationInfo))]

[JsonSerializable(typeof(Modules.BrowsingContext.UserPromptOpenedEventArgs))]
[JsonSerializable(typeof(Modules.BrowsingContext.UserPromptClosedEventArgs))]
[JsonSerializable(typeof(Modules.BrowsingContext.Origin), TypeInfoPropertyName = "BrowsingContext_Origin")]

[JsonSerializable(typeof(Modules.Network.BytesValue.String), TypeInfoPropertyName = "Network_BytesValue_String")]
[JsonSerializable(typeof(Modules.Network.UrlPattern.String), TypeInfoPropertyName = "Network_UrlPattern_String")]
[JsonSerializable(typeof(Modules.Network.ContinueWithAuthParameters.Default), TypeInfoPropertyName = "Network_ContinueWithAuthParameters_Default")]
[JsonSerializable(typeof(Modules.Network.AddInterceptCommand))]
[JsonSerializable(typeof(Modules.Network.AddInterceptResult))]
[JsonSerializable(typeof(Modules.Network.ContinueRequestCommand))]
[JsonSerializable(typeof(Modules.Network.ContinueResponseCommand))]
[JsonSerializable(typeof(Modules.Network.ContinueWithAuthCommand))]
[JsonSerializable(typeof(Modules.Network.FailRequestCommand))]
[JsonSerializable(typeof(Modules.Network.ProvideResponseCommand))]
[JsonSerializable(typeof(Modules.Network.RemoveInterceptCommand))]
[JsonSerializable(typeof(Modules.Network.SetCacheBehaviorCommand))]

[JsonSerializable(typeof(Modules.Network.BeforeRequestSentEventArgs))]
[JsonSerializable(typeof(Modules.Network.ResponseStartedEventArgs))]
[JsonSerializable(typeof(Modules.Network.ResponseCompletedEventArgs))]
[JsonSerializable(typeof(Modules.Network.FetchErrorEventArgs))]
[JsonSerializable(typeof(Modules.Network.AuthRequiredEventArgs))]

[JsonSerializable(typeof(Modules.Script.Channel), TypeInfoPropertyName = "Script_Channel")]
[JsonSerializable(typeof(Modules.Script.LocalValue.String), TypeInfoPropertyName = "Script_LocalValue_String")]
[JsonSerializable(typeof(Modules.Script.Target.Realm), TypeInfoPropertyName = "Script_Target_Realm")]
[JsonSerializable(typeof(Modules.Script.Target.Context), TypeInfoPropertyName = "Script_Target_Context")]

[JsonSerializable(typeof(Modules.Script.AddPreloadScriptCommand))]
[JsonSerializable(typeof(Modules.Script.AddPreloadScriptResult))]
[JsonSerializable(typeof(Modules.Script.DisownCommand))]
[JsonSerializable(typeof(Modules.Script.CallFunctionCommand))]
[JsonSerializable(typeof(Modules.Script.EvaluateCommand))]
[JsonSerializable(typeof(Modules.Script.EvaluateResult))]
[JsonSerializable(typeof(Modules.Script.GetRealmsCommand))]
[JsonSerializable(typeof(Modules.Script.GetRealmsResult))]
[JsonSerializable(typeof(Modules.Script.RemovePreloadScriptCommand))]

[JsonSerializable(typeof(Modules.Script.MessageEventArgs))]
[JsonSerializable(typeof(Modules.Script.RealmDestroyedEventArgs))]
[JsonSerializable(typeof(IReadOnlyList<Modules.Script.RealmInfo>))]

[JsonSerializable(typeof(Modules.Log.Entry))]

[JsonSerializable(typeof(Modules.Storage.GetCookiesCommand))]
[JsonSerializable(typeof(Modules.Storage.GetCookiesResult))]
[JsonSerializable(typeof(Modules.Storage.SetCookieCommand))]
[JsonSerializable(typeof(Modules.Storage.SetCookieResult))]
[JsonSerializable(typeof(Modules.Storage.DeleteCookiesCommand))]
[JsonSerializable(typeof(Modules.Storage.DeleteCookiesResult))]

[JsonSerializable(typeof(Modules.Input.PerformActionsCommand))]
[JsonSerializable(typeof(Modules.Input.ReleaseActionsCommand))]
[JsonSerializable(typeof(Modules.Input.Pointer.Down), TypeInfoPropertyName = "Input_Pointer_Down")]
[JsonSerializable(typeof(Modules.Input.Pointer.Up), TypeInfoPropertyName = "Input_Pointer_Up")]
[JsonSerializable(typeof(Modules.Input.Pointer.Move), TypeInfoPropertyName = "Input_Pointer_Move")]
[JsonSerializable(typeof(Modules.Input.Key.Down), TypeInfoPropertyName = "Input_Key_Down")]
[JsonSerializable(typeof(Modules.Input.Key.Up), TypeInfoPropertyName = "Input_Key_Up")]
[JsonSerializable(typeof(IEnumerable<Modules.Input.IPointerSourceAction>))]
[JsonSerializable(typeof(IEnumerable<Modules.Input.IKeySourceAction>))]
[JsonSerializable(typeof(IEnumerable<Modules.Input.INoneSourceAction>))]
[JsonSerializable(typeof(IEnumerable<Modules.Input.IWheelSourceAction>))]

internal partial class BiDiJsonSerializerContext : JsonSerializerContext;
