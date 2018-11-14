namespace BusinessLib
{
    /// <summary>
    /// A data source that provides raw data objects.  In a real
    /// application this class would make calls to a database.
    /// </summary>
    public static class Database
    {
        #region GetFamilyTree

        public static Person GetFamilyTree()
        {
            // In a real app this method would access a database.
            return new Person
            {
                Name = "David Weatherbeam",
                Children =
                {
                    new Person
                    {
                        Name="Yvette Weatherbeam",
                        Children=
                        {
                            new Person
                            {
                                Name="Zena Hairmonger",
                                Children=
                                {
                                    new Person
                                    {
                                        Name="Sarah Applifunk",
                                        Children=
                                        {
                                            new Person
                                            {
                                                Name="Zena Hairmonger II",
                                                Children=
                                                {
                                                    new Person
                                                    {
                                                        Name="Sarah Applifunk II",
                                                        Children=
                                                        {
                                                            new Person
                                                            {
                                                                Name="Zena Hairmonger III",
                                                                Children=
                                                                {
                                                                    new Person
                                                                    {
                                                                        Name="Zena Hairmonger III",
                                                                        Children=
                                                                        {
                                                                            new Person
                                                                            {
                                                                                Name="Rocky Hairmonger",
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            },
                            new Person
                            {
                                Name="Jenny van Machoqueen",
                                Children=
                                {
                                    new Person
                                    {
                                        Name="Nick van Machoqueen",
                                    },
                                    new Person
                                    {
                                        Name="Matilda Porcupinicus",
                                    },
                                    new Person
                                    {
                                        Name="Bronco van Machoqueen",
                                    }
                                }
                            }
                        }
                    },
                    new Person
                    {
                        Name="Komrade Winkleford",
                        Children=
                        {
                            new Person
                            {
                                Name="Maurice Winkleford",
                                Children=
                                {
                                    new Person
                                    {
                                        Name="Divinity W. Llamafoot",
                                    }
                                }
                            },
                            new Person
                            {
                                Name="Komrade Winkleford, Jr.",
                                Children=
                                {
                                    new Person
                                    {
                                        Name="Saratoga Z. Crankentoe",
                                    },
                                    new Person
                                    {
                                        Name="Excaliber Winkleford",
                                        Children=
                                        {
                                            new Person
                                            {
                                                Name="Tyler Winkleford",
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        #endregion // GetFamilyTree
    }
}