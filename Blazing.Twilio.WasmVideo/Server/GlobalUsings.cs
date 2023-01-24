// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

global using System.Security.Claims;
global using Blazing.Twilio.WasmVideo.Server;
global using Blazing.Twilio.WasmVideo.Server.Hubs;
global using Blazing.Twilio.WasmVideo.Server.Options;
global using Blazing.Twilio.WasmVideo.Server.Services;
global using Blazing.Twilio.WasmVideo.Shared;
global using Microsoft.AspNetCore.Http.Features;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.ResponseCompression;
global using Microsoft.AspNetCore.SignalR;
global using Microsoft.Net.Http.Headers;
global using Twilio;
global using Twilio.Base;
global using Twilio.Jwt.AccessToken;
global using Twilio.Rest.Video.V1;
global using Twilio.Rest.Video.V1.Room;
global using MicrosoftOptions = Microsoft.Extensions.Options;
global using ParticipantStatus = Twilio.Rest.Video.V1.Room.ParticipantResource.StatusEnum;