﻿// Copyright © 2020 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Platformus.Website.Data.Entities;

namespace Platformus.Website.Backend.ViewModels.Tabs
{
  public static class CreateOrEditViewModelFactory
  {
    public static CreateOrEditViewModel Create(Tab tab)
    {
      if (tab == null)
        return new CreateOrEditViewModel()
        {
        };

      return new CreateOrEditViewModel()
      {
        Id = tab.Id,
        Name = tab.Name,
        Position = tab.Position
      };
    }
  }
}