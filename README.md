# Noah's Ark - plug-in for Grasshopper
A Grasshopper plug-in to save endangered animals from the rising waters. 

_Noah’s Ark_ is an open-source, crowd-sourced Grasshopper plug-in maintained by ShapeDiver and frequently shipped to ShapeDiver systems. 
It provides the possibility to:

  * Rescue legacy plug-in functionality, 
  * Develop and distribute new full-fledged Grasshopper components without having to maintain a plug-in, and 
  * Ship plug-in functionality to ShapeDiver without the need for an Enterprise account. 

--> Noah’s Ark is already available in Rhino’s package manager and on [food4rhino](https://www.food4rhino.com/en/app/noahsark).

## Motivation

At ShapeDiver, we already support about 40+ plug-ins and frequently receive requests from our users to deploy more ones to our systems. 
We must be picky and careful about the plug-ins we deploy, always keeping security and maintenance in mind. 
Therefore, our plug-in reviewing process is extensive, and sometimes, we have to refrain from deploying a specific plug-in or plug-in update. 
You can read more about plug-in reviewing [here](https://help.shapediver.com/doc/guidelines-for-plugin-developers). 

Since Rhino 8, a new challenge has entered the stage: Moving Rhino to .NET 7 (read about it [here](https://developer.rhino3d.com/guides/rhinocommon/moving-to-dotnet-7/)). 
In future versions of Rhino, this might leave old widespread plug-ins behind. 

For these reasons, we initiated _Noah’s Ark_. 

## Functionality

The initial release contains a component for approximate solutions to the [Cutting stock problem](https://en.wikipedia.org/wiki/Cutting_stock_problem) (1D cutoff optimization). The source code was adapted from this [repository](https://github.com/AlexanderMorozovDesign/GH_Linear_Cutting) and integrated into Noah's Ark with the author's permission, [Alexander Morozov](https://github.com/AlexanderMorozovDesign). 

## How to contribute

Fork and clone the repository, develop your extension to Noah's Ark, and open a [pull request](https://github.com/shapediver/NoahsArk/pulls). 
We will review your pull request and provide feedback quickly. If you want to discuss before starting to code, please open an [issue](https://github.com/shapediver/NoahsArk/issues).

## Related

Should you prefer to develop your own plug-in, check out our [Grasshopper and Rhino plug-in template](https://github.com/shapediver/GrasshopperPluginTemplate).

Would you like us to include further functionality in Noah’s Ark? We are looking forward to your feedback and comments 

  * on our [forum](https://forum.shapediver.com), 
  * using [GitHub issues](https://github.com/shapediver/NoahsArk/issues), or 
  * by joining one of our [community standups or onboarding sessions](https://www.shapediver.com/app/community). 

