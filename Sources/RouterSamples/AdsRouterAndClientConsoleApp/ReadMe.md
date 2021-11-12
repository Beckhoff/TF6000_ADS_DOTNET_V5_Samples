# AdsRouterAndClientConsoleApp
This sample shows how to establish an AmsTcpIpRouter and an AdsClient together in one console app.

This solution has two main entry points that can be activated by the project properties:

## 1. AdsRouterAndClientConsoleApp.SimpleProgram
This demonstrates how to run the AmsTcpIpRouter asynchronously from the AdsClient in a very simple way.
ADS configurations are taken directly from code.

## 2. AdsRouterAndClientConsoleApp.UseServices
This demonstrates how to wrap the AmsTcpIpRouter and two independent AdsClients into IHostedService (BackgroundService) to run them in different Host environments as (Micro)-Services (e.g Azure, Docker etc.).
Application Settings are taken from different Configuration sources.