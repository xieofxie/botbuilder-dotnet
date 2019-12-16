﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma warning disable SA1124 // Do not use regions

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Bot.Expressions;
using Microsoft.Bot.Expressions.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Expressions.Tests
{
    [TestClass]
    public class ExpressionEngineTests
    {
        private static readonly string NullStr = null;

        private readonly object scope = new Dictionary<string, object>
        {
            {
                "path", new Dictionary<string, object>()
                {
                    {
                        "array", new List<int>() { 1 }
                    }
                }
            },
            { "one", 1.0 },
            { "two", 2.0 },
            { "hello", "hello" },
            { "world", "world" },
            { "cit", "cit" },
            { "y", "y" },
            { "istrue", true },
            { "nullObj", NullStr },
            {
                "bag", new Dictionary<string, object>
                {
                    { "three", 3.0 },
                    { "set", new { four = 4.0 } },
                    { "list", new[] { "red", "blue" } },
                    { "index", 3 },
                    { "name", "mybag" }
                }
            },
            { "items", new string[] { "zero", "one", "two" } },
            {
                "nestedItems",
                new[]
                {
                    new { x = 1 },
                    new { x = 2 },
                    new { x = 3 }
                }
            },
            {
                "user",
                new
                {
                    lists = new
                    {
                        todo = new[]
                        {
                            "todo1",
                            "todo2",
                            "todo3",
                        }
                    },
                    listType = "todo",
                }
            },
            { "timestamp", "2018-03-15T13:00:00.000Z" },
            { "notISOTimestamp", "2018/03/15 13:00:00" },
            { "timestampObj", DateTime.Parse("2018-03-15T13:00:00.000Z").ToUniversalTime() },
            { "unixTimestamp", 1521118800 },
            { "xmlStr", "<?xml version='1.0'?> <produce> <item> <name>Gala</name> <type>apple</type> <count>20</count> </item> <item> <name>Honeycrisp</name> <type>apple</type> <count>10</count> </item> </produce>" },
            {
                "jsonStr", @"{
                          'Stores': [
                            'Lambton Quay',
                            'Willis Street'
                          ],
                          'Manufacturers': [
                            {
                              'Name': 'Acme Co',
                              'Products': [
                                {
                                  'Name': 'Anvil',
                                  'Price': 50
                                }
                              ]
                            },
                            {
                              'Name': 'Contoso',
                              'Products': [
                                {
                                  'Name': 'Elbow Grease',
                                  'Price': 99.95
                                },
                                {
                                  'Name': 'Headlight Fluid',
                                  'Price': 4
                                }
                              ]
                            }
                          ]
                        }"
            },
            {
                "turn", new
                {
                    recognized = new
                    {
                        entities = new Dictionary<string, object>
                        {
                            {
                                "city",
                                new[]
                                {
                                    "Seattle"
                                }
                            },
                            {
                                "ordinal",
                                new[]
                                {
                                    "1",
                                    "2",
                                    "3"
                                }
                            },
                            {
                                "CompositeList1",
                                new[]
                                {
                                    new[]
                                    {
                                        "firstItem"
                                    }
                                }
                            },
                            {
                                "CompositeList2",
                                new[]
                                {
                                    new[]
                                    {
                                        "firstItem",
                                        "secondItem"
                                    }
                                }
                            }
                        },
                        intents = new
                        {
                            BookFlight = "BookFlight",
                            BookHotel = new[]
                            {
                                new
                                {
                                    Where = "Bellevue",
                                    Time = "Tomorrow",
                                    People= "2"
                                },
                                new
                                {
                                    Where = "Kirkland",
                                    Time = "Today",
                                    People = "4"
                                }
                            }
                        }
                    }
                }
            },
            {
                "dialog",
                new
                {
                    x=3,
                    instance = new
                    {
                        xxx = "instance",
                        yyy = new
                        {
                            instanceY = "instanceY"
                        }
                    },
                    options = new
                    {
                        xxx = "options",
                        yyy = new[] { "optionY1", "optionY2" }
                    },
                    title = "Dialog Title",
                    subTitle = "Dialog Sub Title"
                }
            },
            {
                "callstack", new object[]
                {
                    new
                    {
                        x = 3,
                        instance = new
                        {
                            xxx = "instance",
                            yyy = new
                            {
                                instanceY = "instanceY"
                            }
                        },
                        options = new
                        {
                            xxx = "options",
                            yyy = new[] { "optionY1", "optionY2" }
                        },
                        title = "Dialog Title",
                        subTitle = "Dialog Sub Title"
                    },
                    new { x = 2, y = 2 },
                    new { x = 1, y = 1, z = 1 }
                }
            }
        };

        public static HashSet<string> One { get; set; } = new HashSet<string> { "one" };

        public static HashSet<string> OneTwo { get; set; } = new HashSet<string> { "one", "two" };

        public static IEnumerable<object[]> Data => new[]
        {
            #region SetPathToProperty test
            Test("setPathToValue(path.simple, 3) + path.simple", 6),
            Test("setPathToValue(path.simple, 5) + path.simple", 10),
            Test("setPathToValue(path.array[0], 7) + path.array[0]", 14),
            Test("setPathToValue(path.array[1], 9) + path.array[1]", 18),

            //Test("setPathToValue(path.darray[2][0], 11) + path.darray[2][0]", 22),
            //Test("setPathToValue(path.darray[2][3].foo, 13) + path.darray[2][3].foo", 26),
            //Test("setPathToValue(path.overwrite, 3) + setPathToValue(path.overwrite[0], 4) + path.overwrite[0]", 11),
            //Test("setPathToValue(path.overwrite[0], 3) + setPathToValue(path.overwrite, 4) + path.overwrite", 11),
            //Test("setPathToValue(path.overwrite.prop, 3) + setPathToValue(path.overwrite, 4) + path.overwrite", 11),
            //Test("setPathToValue(path.overwrite.prop, 3) + setPathToValue(path.overwrite[0], 4) + path.overwrite[0]", 11),
            Test("setPathToValue(path.x, null)", null),
            #endregion

            #region Operators test
            Test("1 + 2", 3),
            Test("- 1 + 2", 1),
            Test("+ 1 + 2", 3),
            Test("1 - 2", -1),
            Test("1 - (-2)", 3),
            Test("1.0 + 2.0", 3.0),
            Test("1 * 2 + 3", 5),
            Test("1 + 2 * 3", 7),
            Test("4 / 2", 2),
            Test("1 + 3 / 2", 2),
            Test("(1 + 3) / 2", 2),
            Test("1 * (2 + 3)", 5),
            Test("(1 + 2) * 3", 9),
            Test("(one + two) * bag.three", 9.0, new HashSet<string> { "one", "two", "bag.three" }),
            Test("(one + two) * bag.set.four", 12.0, new HashSet<string> { "one", "two", "bag.set.four" }),

            Test("2^2", 4.0),
            Test("3^2^2", 81.0),
            Test("one > 0.5 && two < 2.5", true),
            Test("one > 0.5 || two < 1.5", true),
            Test("5 % 2", 1),
            Test("!(one == 1.0)", false),
            Test("!!(one == 1.0)", true),
            Test("!exists(xione) || !!exists(two)", true),
            Test("(1 + 2) == (4 - 1)", true),
            Test("!!exists(one) == !!exists(one)", true),
            Test("!(one == 1.0)", false, new HashSet<string> { "one" }),
            Test("!!(one == 1.0)", true, new HashSet<string> { "one" }),
            Test("!(one == 1.0) || !!(two == 2.0)", true, OneTwo),
            Test("!true", false),
            Test("!!true", true),
            Test("!(one == 1.0) || !!(two == 2.0)", true),
            Test("hello == 'hello'", true),
            Test("hello == 'world'", false),
            Test("(1 + 2) != (4 - 1)", false),
            Test("!!exists(one) != !!exists(one)", false),
            Test("hello != 'hello'", false),
            Test("hello != 'world'", true),
            Test("hello != \"hello\"", false),
            Test("hello != \"world\"", true),
            Test("(1 + 2) >= (4 - 1)", true),
            Test("(2 + 2) >= (4 - 1)", true),
            Test("float(5.5) >= float(4 - 1)", true),
            Test("(1 + 2) <= (4 - 1)", true),
            Test("(2 + 2) <= (4 - 1)", false),
            Test("float(5.5) <= float(4 - 1)", false),
            Test("'string'&'builder'", "stringbuilder"),
            Test("\"string\"&\"builder\"", "stringbuilder"),
            Test("one > 0.5 && two < 2.5", true, OneTwo),
            Test("notThere > 4", false),
            Test("float(5.5) && float(0.0)", true),
            Test("hello && \"hello\"", true),
            Test("items || ((2 + 2) <= (4 - 1))", true), // true || false
            Test("0 || false", true), // true || false
            Test("!(hello)", false), // false
            Test("!(10)", false),
            Test("!(0)", false),
            Test("one > 0.5 || two < 1.5", true, OneTwo),
            Test("one / 0 || two", true),
            Test("0/3", 0),
            Test("True == true", true),
            #endregion

            #region  String functions test
            Test("concat(hello,world)", "helloworld"),
            Test("concat('hello','world')", "helloworld"),
            Test("concat(nullObj,'world')", "world"),
            Test("concat('hello',nullObj)", "hello"),
            Test("concat(\"hello\",\"world\")", "helloworld"),
            Test("length('hello')", 5),
            Test("length(\"hello\")", 5),
            Test("length(nullObj)", 0),
            Test("length(concat(hello,world))", 10),
            Test("count('hello')", 5),
            Test("count(\"hello\")", 5),
            Test("count(concat(hello,world))", 10),
            Test("replace('hello', 'l', 'k')", "hekko"),
            Test("replace('hello', 'L', 'k')", "hello"),
            Test("replace(nullObj, 'L', 'k')", string.Empty),
            Test("replace('hello', 'L', 'k')", "hello"),
            Test("replace('hello', 'l', nullObj)", "heo"),
            Test("replace(\"hello'\", \"'\", '\"')", "hello\""),
            Test("replace('hello\"', '\"', \"'\")", "hello'"),
            Test("replace('hello\"', '\"', '\n')", "hello\n"),
            Test("replace('hello\n', '\n', '\\\\')", "hello\\"),
            Test(@"replace('hello\\', '\\', '\\\\')", @"hello\\"),
            Test(@"replace('hello\n', '\n', '\\\\')", @"hello\\"),
            Test("replaceIgnoreCase('hello', 'L', 'k')", "hekko"),
            Test("replaceIgnoreCase(nullObj, 'L', 'k')", string.Empty),
            Test("split('hello','e')", new string[] { "h", "llo" }),
            Test("split(nullObj,'e')", new string[] { string.Empty }),

            //Test("split('hello',nullObj)", new string[] { "h", "e", "l", "l", "o" }),
            Test("split('hello',nullObj)", new string[] { "hello" }),
            Test("substring('hello', 0, 5)", "hello"),
            Test("substring('hello', 0, 3)", "hel"),
            Test("substring('hello', 3)", "lo"),
            Test("substring(nullObj, 3)", string.Empty),
            Test("substring('hello', 0, bag.index)", "hel"),
            Test("toLower('UpCase')", "upcase"),
            Test("toLower(nullObj)", string.Empty),
            Test("toUpper('lowercase')", "LOWERCASE"),
            Test("toUpper(nullObj)", string.Empty),
            Test("toLower(toUpper('lowercase'))", "lowercase"),
            Test("trim(' hello ')", "hello"),
            Test("trim(' hello')", "hello"),
            Test("trim(nullObj)", string.Empty),
            Test("trim('hello')", "hello"),
            Test("endsWith('hello','o')", true),
            Test("endsWith('hello','a')", false),
            Test("endsWith(hello,'o')", true),
            Test("endsWith(hello,'a')", false),
            Test("endsWith(nullObj,'h')", false),
            Test("endsWith('hello', nullObj)", true),
            Test("startsWith('hello','h')", true),
            Test("startsWith(nullObj,'h')", false),
            Test("startsWith('hello', nullObj)", true),
            Test("startsWith('hello','a')", false),
            Test("countWord(hello)", 1),
            Test("countWord(nullObj)", 0),
            Test("countWord(concat(hello, ' ', world))", 2),
            Test("addOrdinal(11)", "11th"),
            Test("addOrdinal(11 + 1)", "12th"),
            Test("addOrdinal(11 + 2)", "13th"),
            Test("addOrdinal(11 + 10)", "21st"),
            Test("addOrdinal(11 + 11)", "22nd"),
            Test("addOrdinal(11 + 12)", "23rd"),
            Test("addOrdinal(11 + 13)", "24th"),
            Test("addOrdinal(-1)", "-1"), // original string value
            Test("join(createArray('a','b', 'c', 'd'), '\n')", "a\nb\nc\nd"),
            
            #endregion

            #region  Logical comparison functions test
            Test("and(1 == 1, 1 < 2, 1 > 2)", false),
            Test("and(!true, !!true)", false), // false && true
            Test("and(!!true, !!true)", true), // true && true
            Test("and(hello != 'world', bool('true'))", true), // true && true
            Test("and(hello == 'world', bool('true'))", false), // false && true
            Test("or(!exists(one), !!exists(one))", true), // false && true
            Test("or(!exists(one), !exists(one))", false), // false && false
            Test("greater(one, two)", false, OneTwo),
            Test("greater(one , 0.5) && less(two , 2.5)", true), // true && true
            Test("greater(one , 0.5) || less(two , 1.5)", true), // true || false
            Test("greater(5, 2)", true),
            Test("greater(2, 2)", false),
            Test("greater(one, two)", false),
            Test("greaterOrEquals((1 + 2) , (4 - 1))", true),
            Test("greaterOrEquals((2 + 2) , (4 - 1))", true),
            Test("greaterOrEquals(float(5.5) , float(4 - 1))", true),
            Test("greaterOrEquals(one, one)", true),
            Test("greaterOrEquals(one, two)", false),
            Test("greaterOrEquals(one, one)", true, One),
            Test("greaterOrEquals(one, two)", false, OneTwo),
            Test("less(5, 2)", false),
            Test("less(2, 2)", false),
            Test("less(one, two)", true),
            Test("less(one, two)", true, OneTwo),
            Test("lessOrEquals(one, one)", true, new HashSet<string> { "one" }),
            Test("lessOrEquals(one, two)", true, OneTwo),
            Test("lessOrEquals(one, one)", true),
            Test("lessOrEquals(one, two)", true),
            Test("lessOrEquals((1 + 2) , (4 - 1))", true),
            Test("lessOrEquals((2 + 2) , (4 - 1))", false),
            Test("lessOrEquals(float(5.5) , float(4 - 1))", false),
            Test("lessOrEquals(one, one)", true),
            Test("lessOrEquals(one, two)", true),
            Test("equals(hello, 'hello')", true),
            Test("equals(bag.index, 3)", true),
            Test("equals(bag.index, 2)", false),
            Test("equals(hello == 'world', bool('true'))", false), // false, true
            Test("equals(hello == 'world', bool(0))", false), // false, true
            Test("if(!exists(one), 'r1', 'r2')", "r2"), // false
            Test("if(!!exists(one), 'r1', 'r2')", "r1"), // true
            Test("if(0, 'r1', 'r2')", "r1"), // true
            Test("if(bool('true'), 'r1', 'r2')", "r1"), // true
            Test("if(istrue, 'r1', 'r2')", "r1"), // true
            Test("if(bag.name == null, \"hello\",  bag.name)", "mybag"),
            Test("if(user.name == null, \"hello\",  user.name)", "hello"), // user.name don't exist
            Test("if(user.name == null, '',  user.name)", string.Empty), // user.name don't exist
            Test("if(one > 0, one, two)", 1),
            Test("if(one < 0, one, two)", 2),
            Test("exists(one)", true),
            Test("exists(xxx)", false),
            Test("exists(one.xxx)", false),
            Test("not(one != null)", false),
            Test("not(not(one != null))", true),
            Test("not(false)", true),
            Test("not(one == 1.0)", false, new HashSet<string> { "one" }),
            Test("not(not(one == 1.0))", true, new HashSet<string> { "one" }),
            Test("not(false)", true),
            Test("and(one > 0.5, two < 2.5)", true, OneTwo),
            Test("and(float(5.5), float(0.0))", true),
            Test("and(hello, \"hello\")", true),
            Test("or(items, (2 + 2) <= (4 - 1))", true), // true || false
            Test("or(0, false)", true), // true || false
            Test("not(hello)", false), // false
            Test("not(10)", false),
            Test("not(0)", false),
            Test("if(hello, 'r1', 'r2')", "r1"),
            Test("if(null, 'r1', 'r2')", "r2"),
            Test("if(hello * 5, 'r1', 'r2')", "r2"),
            Test("if(10, 'r1', 'r2')", "r1"),
            #endregion

            #region  Conversion functions test
            Test("float('10.333')", 10.333f),
            Test("float('10')", 10.0f),
            Test("int('10')", 10),
            Test("string('str')", "str"),
            Test("string(one)", "1.0"),
            Test("string(bool(1))", "true"),
            Test("string(bag.set)", "{\"four\":4.0}"),
            Test("bool(1)", true),
            Test("bool(0)", true),
            Test("bool(null)", false),
            Test("bool(hello * 5)", false),
            Test("bool('false')", true),
            Test("bool('hi')", true),
            Test("createArray('h', 'e', 'l', 'l', 'o')", new List<object> { "h", "e", "l", "l", "o" }),
            Test("createArray(1, bool(0), string(bool(1)), float('10'))", new List<object> { 1, true, "true", 10.0f }),
            Test("array('hello')", new List<object> { "hello" }),
            Test("binary(hello)", "0110100001100101011011000110110001101111"),
            Test("length(binary(hello))", 40),
            Test("base64(hello)", "aGVsbG8="),
            Test("base64ToBinary(base64(hello))", "0110000101000111010101100111001101100010010001110011100000111101"),
            Test("base64ToString(base64(hello))", "hello"),
            Test("dataUri(hello)", "data:text/plain;charset=utf-8;base64,aGVsbG8="),
            Test("dataUriToBinary(base64(hello))", "0110000101000111010101100111001101100010010001110011100000111101"),
            Test("dataUriToString(dataUri(hello))", "hello"),
            Test("xml('{\"person\": {\"name\": \"Sophia Owen\", \"city\": \"Seattle\"}}')", $"<root type=\"object\">{Environment.NewLine}  <person type=\"object\">{Environment.NewLine}    <name type=\"string\">Sophia Owen</name>{Environment.NewLine}    <city type=\"string\">Seattle</city>{Environment.NewLine}  </person>{Environment.NewLine}</root>"),
            Test("uriComponent('http://contoso.com')", "http%3A%2F%2Fcontoso.com"),
            Test("uriComponentToString('http%3A%2F%2Fcontoso.com')", "http://contoso.com"),
            #endregion

            #region  Math functions test
            Test("add(1, 2, 3)", 6),
            Test("add(1, 2)", 3),
            Test("add(1.0, 2.0)", 3.0),
            Test("add(mul(1, 2), 3)", 5),
            Test("max(mul(1, 2), 5) ", 5),
            Test("max(5) ", 5),
            Test("max(4, 5) ", 5),
            Test("min(mul(1, 2), 5) ", 2),
            Test("min(4, 5) ", 4),
            Test("min(4) ", 4),
            Test("min(1.0, two) + max(one, 2.0)", 3.0, OneTwo),
            Test("sub(2, 1)", 1),
            Test("sub(2, 1, 1)", 0),
            Test("sub(2.0, 0.5)", 1.5),
            Test("mul(2, 5)", 10),
            Test("mul(2, 5, 2)", 20),
            Test("div(mul(2, 5), 2)", 5),
            Test("div(5, 2)", 2),
            Test("div(5, 2 ,2)", 1),
            Test("exp(2,2)", 4.0),
            Test("mod(5,2)", 1),
            Test("rand(1, 2)", 1),
            Test("rand(2, 3)", 2),
            Test("range(1,4)", new[] { 1, 2, 3, 4 }),
            Test("range(-1,6)", new[] { -1, 0, 1, 2, 3, 4 }),
            #endregion

            #region  Date and time function test

            // init dateTime: 2018-03-15T13:00:00Z
            Test("addDays(timestamp, 1)", "2018-03-16T13:00:00.000Z"),
            Test("addDays(timestamp, 1,'MM-dd-yy')", "03-16-18"),
            Test("addHours(timestamp, 1)", "2018-03-15T14:00:00.000Z"),
            Test("addHours(timestamp, 1,'MM-dd-yy hh-mm')", "03-15-18 02-00"),
            Test("addMinutes(timestamp, 1)", "2018-03-15T13:01:00.000Z"),
            Test("addMinutes(timestamp, 1, 'MM-dd-yy hh-mm')", "03-15-18 01-01"),
            Test("addSeconds(timestamp, 1)", "2018-03-15T13:00:01.000Z"),
            Test("addSeconds(timestamp, 1, 'MM-dd-yy hh-mm-ss')", "03-15-18 01-00-01"),
            Test("dayOfMonth(timestamp)", 15),
            Test("dayOfWeek(timestamp)", 4), // Thursday
            Test("dayOfYear(timestamp)", 74),
            Test("month(timestamp)", 3),
            Test("date(timestamp)", "3/15/2018"), // Default. TODO
            Test("year(timestamp)", 2018),
            Test("length(utcNow())", 24),
            Test("utcNow('MM-DD-YY')", DateTime.UtcNow.ToString("MM-DD-YY")),
            Test("formatDateTime(notISOTimestamp)", "2018-03-15T13:00:00.000Z"),
            Test("formatDateTime(notISOTimestamp, 'MM-dd-yy')", "03-15-18"),
            Test("formatDateTime('2018-03-15')", "2018-03-15T00:00:00.000Z"),
            Test("formatDateTime(timestampObj)", "2018-03-15T13:00:00.000Z"),
            Test("formatDateTime(unixTimestamp)", "2018-03-15T13:00:00.000Z"),
            Test("subtractFromTime(timestamp, 1, 'Year')", "2017-03-15T13:00:00.000Z"),
            Test("subtractFromTime(timestamp, 1, 'Month')", "2018-02-15T13:00:00.000Z"),
            Test("subtractFromTime(timestamp, 1, 'Week')", "2018-03-08T13:00:00.000Z"),
            Test("subtractFromTime(timestamp, 1, 'Day')", "2018-03-14T13:00:00.000Z"),
            Test("subtractFromTime(timestamp, 1, 'Hour')", "2018-03-15T12:00:00.000Z"),
            Test("subtractFromTime(timestamp, 1, 'Minute')", "2018-03-15T12:59:00.000Z"),
            Test("subtractFromTime(timestamp, 1, 'Second')", "2018-03-15T12:59:59.000Z"),
            Test("dateReadBack(timestamp, addDays(timestamp, 1))", "tomorrow"),
            Test("dateReadBack(addDays(timestamp, 1),timestamp)", "yesterday"),
            Test("getTimeOfDay('2018-03-15T00:00:00.000Z')", "midnight"),
            Test("getTimeOfDay('2018-03-15T08:00:00.000Z')", "morning"),
            Test("getTimeOfDay('2018-03-15T12:00:00.000Z')", "noon"),
            Test("getTimeOfDay('2018-03-15T13:00:00.000Z')", "afternoon"),
            Test("getTimeOfDay('2018-03-15T18:00:00.000Z')", "evening"),
            Test("getTimeOfDay('2018-03-15T22:00:00.000Z')", "evening"),
            Test("getTimeOfDay('2018-03-15T23:00:00.000Z')", "night"),
            Test("getPastTime(1,'Year','MM-dd-yy')", DateTime.Now.AddYears(-1).ToString("MM-dd-yy")),
            Test("getPastTime(1,'Month','MM-dd-yy')", DateTime.Now.AddMonths(-1).ToString("MM-dd-yy")),
            Test("getPastTime(1,'Week','MM-dd-yy')", DateTime.Now.AddDays(-7).ToString("MM-dd-yy")),
            Test("getPastTime(1,'Day','MM-dd-yy')", DateTime.Now.AddDays(-1).ToString("MM-dd-yy")),
            Test("getFutureTime(1,'Year','MM-dd-yy')", DateTime.Now.AddYears(1).ToString("MM-dd-yy")),
            Test("getFutureTime(1,'Month','MM-dd-yy')", DateTime.Now.AddMonths(1).ToString("MM-dd-yy")),
            Test("getFutureTime(1,'Week','MM-dd-yy')", DateTime.Now.AddDays(7).ToString("MM-dd-yy")),
            Test("getFutureTime(1,'Day','MM-dd-yy')", DateTime.Now.AddDays(1).ToString("MM-dd-yy")),
            Test("convertFromUTC('2018-01-02T02:00:00.000Z', 'Pacific Standard Time', 'D')", "Monday, January 1, 2018"),
            Test("convertFromUTC('2018-01-02T01:00:00.000Z', 'America/Los_Angeles', 'D')", "Monday, January 1, 2018"),
            Test("convertToUTC('01/01/2018 00:00:00', 'Pacific Standard Time')", "2018-01-01T08:00:00.000Z"),
            Test("addToTime('2018-01-01T08:00:00.000Z', 1, 'Day', 'D')", "Tuesday, January 2, 2018"),
            Test("addToTime('2018-01-01T00:00:00.000Z', 1, 'Week')", "2018-01-08T00:00:00.000Z"),
            Test("startOfDay('2018-03-15T13:30:30.000Z')", "2018-03-15T00:00:00.000Z"),
            Test("startOfHour('2018-03-15T13:30:30.000Z')", "2018-03-15T13:00:00.000Z"),
            Test("startOfMonth('2018-03-15T13:30:30.000Z')", "2018-03-01T00:00:00.000Z"),
            Test("ticks('2018-01-01T08:00:00.000Z')", 636503904000000000),
            #endregion

            #region uri parsing function test
            Test("uriHost('https://www.localhost.com:8080')", "www.localhost.com"),
            Test("uriPath('http://www.contoso.com/catalog/shownew.htm?date=today')", "/catalog/shownew.htm"),
            Test("uriPathAndQuery('http://www.contoso.com/catalog/shownew.htm?date=today')", "/catalog/shownew.htm?date=today"),
            Test("uriPort('http://www.localhost:8080')", 8080),
            Test("uriQuery('http://www.contoso.com/catalog/shownew.htm?date=today')", "?date=today"),
            Test("uriScheme('http://www.contoso.com/catalog/shownew.htm?date=today')", "http"),
            #endregion

            #region  collection functions test
            Test("sum(createArray(1, 2))", 3),
            Test("sum(createArray(one, two, 3))", 6.0),
            Test("average(createArray(1, 2))", 1.5),
            Test("average(createArray(one, two, 3))", 2.0),
            Test("contains('hello world', 'hello')", true),
            Test("contains('hello world', 'hellow')", false),
            Test("contains(items, 'zero')", true),
            Test("contains(items, 'hi')", false),
            Test("contains(bag, 'three')", true),
            Test("contains(bag, 'xxx')", false),
            Test("count(split(hello,'e'))", 2),
            Test("count(createArray('h', 'e', 'l', 'l', 'o'))", 5),
            Test("empty('')", true),
            Test("empty('a')", false),
            Test("empty(bag)", false),
            Test("empty(items)", false),
            Test("first(items)", "zero"),
            Test("first('hello')", "h"),
            Test("first(createArray(0, 1, 2))", 0),
            Test("first(1)", null),
            Test("first(nestedItems).x", 1, new HashSet<string> { "nestedItems" }),
            Test("join(items,',')", "zero,one,two"),
            Test("join(createArray('a', 'b', 'c'), '.')", "a.b.c"),
            Test("join(createArray('a', 'b', 'c'), ',', ' and ')", "a,b and c"),
            Test("join(createArray('a', 'b'), ',', ' and ')", "a and b"),
            Test("join(foreach(items, item, item), ',')", "zero,one,two"),
            Test("join(foreach(nestedItems, i, i.x + first(nestedItems).x), ',')", "2,3,4", new HashSet<string> { "nestedItems" }),
            Test("join(foreach(items, item, concat(item, string(count(items)))), ',')", "zero3,one3,two3", new HashSet<string> { "items" }),
            Test("join(select(items, item, item), ',')", "zero,one,two"),
            Test("join(select(nestedItems, i, i.x + first(nestedItems).x), ',')", "2,3,4", new HashSet<string> { "nestedItems" }),
            Test("join(select(items, item, concat(item, string(count(items)))), ',')", "zero3,one3,two3", new HashSet<string> { "items" }),
            Test("join(where(items, item, item == 'two'), ',')", "two"),
            Test("join(foreach(where(nestedItems, item, item.x > 1), result, result.x), ',')", "2,3", new HashSet<string> { "nestedItems" }),
            Test("last(items)", "two"),
            Test("last('hello')", "o"),
            Test("last(createArray(0, 1, 2))", 2),
            Test("last(1)", null),
            Test("count(union(createArray('a', 'b')))", 2),
            Test("count(union(createArray('a', 'b'), createArray('b', 'c'), createArray('b', 'd')))", 4),
            Test("count(intersection(createArray('a', 'b')))", 2),
            Test("count(intersection(createArray('a', 'b'), createArray('b', 'c'), createArray('b', 'd')))", 1),
            Test("skip(createArray('H','e','l','l','0'),2)", new List<object> { "l", "l", "0" }),
            Test("take(createArray('H','e','l','l','0'),2)", new List<object> { "H", "e" }),
            Test("subArray(createArray('H','e','l','l','o'),2,5)", new List<object> { "l", "l", "o" }),
            Test("count(newGuid())", 36),
            Test("indexOf(newGuid(), '-')", 8),
            Test("indexOf(nullObj, '-')", -1),
            Test("indexOf(hello, nullObj)", 0),
            Test("indexOf(hello, '-')", -1),
            Test("lastIndexOf(newGuid(), '-')", 23),
            Test("lastIndexOf(hello, '-')", -1),
            Test("lastIndexOf(nullObj, '-')", -1),
            Test("lastIndexOf(hello, nullObj)", 4),
            Test("length(newGuid())", 36),
            Test("sortBy(items)", new List<object> { "one", "two", "zero" }),
            Test("sortBy(nestedItems, 'x')[0].x", 1),
            Test("sortByDescending(items)", new List<object> { "zero", "two", "one" }),
            Test("sortByDescending(nestedItems, 'x')[0].x", 3),
            #endregion

            #region  Object manipulation and construction functions
            Test("string(addProperty(json('{\"key1\":\"value1\"}'), 'key2','value2'))", "{\"key1\":\"value1\",\"key2\":\"value2\"}"),
            Test("string(setProperty(json('{\"key1\":\"value1\"}'), 'key1','value2'))", "{\"key1\":\"value2\"}"),
            Test("string(removeProperty(json('{\"key1\":\"value1\",\"key2\":\"value2\"}'), 'key2'))", "{\"key1\":\"value1\"}"),
            Test("coalesce(nullObj,hello,nullObj)", "hello"),
            Test("xPath(xmlStr,'/produce/item/name')", new[] { "<name>Gala</name>", "<name>Honeycrisp</name>" }),
            Test("xPath(xmlStr,'sum(/produce/item/count)')", 30),
            Test("jPath(jsonStr,'Manufacturers[0].Products[0].Price')", 50),
            Test("jPath(jsonStr,'$..Products[?(@.Price >= 50)].Name')", new[] { "Anvil", "Elbow Grease" }),
            #endregion

            #region  Memory access
            Test("getProperty(bag, concat('na','me'))", "mybag"),
            Test("items[2]", "two", new HashSet<string> { "items[2]" }),
            Test("bag.list[bag.index - 2]", "blue", new HashSet<string> { "bag.list", "bag.index" }),
            Test("items[nestedItems[1].x]", "two", new HashSet<string> { "items", "nestedItems[1].x" }),
            Test("bag['name']", "mybag"),
            Test("bag[substring(concat('na','me','more'), 0, length('name'))]", "mybag"),
            Test("items[1+1]", "two"),
            Test("getProperty(null, 'p')", null),
            Test("(getProperty(null, 'p'))[1]", null),
            #endregion

            #region Regex
            Test("isMatch('abc', '^[ab]+$')", false), // simple character classes ([abc]), "+" (one or more)
            Test("isMatch('abb', '^[ab]+$')", true), // simple character classes ([abc])
            Test("isMatch('123', '^[^abc]+$')", true), // complemented character classes ([^abc])
            Test("isMatch('12a', '^[^abc]+$')", false), // complemented character classes ([^abc])
            Test("isMatch('123', '^[^a-z]+$')", true), // complemented character classes ([^a-z])
            Test("isMatch('12a', '^[^a-z]+$')", false), // complemented character classes ([^a-z])
            Test("isMatch('a1', '^[a-z]?[0-9]$')", true), // "?" (zero or one)
            Test("isMatch('1', '^[a-z]?[0-9]$')", true), // "?" (zero or one)
            Test("isMatch('1', '^[a-z]*[0-9]$')", true), // "*" (zero or more)
            Test("isMatch('abc1', '^[a-z]*[0-9]$')", true), // "*" (zero or more)
            Test("isMatch('ab', '^[a-z]{1}$')", false), // "{x}" (exactly x occurrences)
            Test("isMatch('ab', '^[a-z]{1,2}$')", true), // "{x,y}" (at least x, at most y, occurrences)
            Test("isMatch('abc', '^[a-z]{1,}$')", true), // "{x,}" (x occurrences or more)
            Test("isMatch('Name', '^(?i)name$')", true), // "(?i)x" (x ignore case)
            Test("isMatch('FORTUNE', '(?i)fortune|future')", true), // "x|y" (alternation)
            Test("isMatch('FUTURE', '(?i)fortune|future')", true), // "x|y" (alternation)
            Test("isMatch('A', '(?i)fortune|future')", false), // "x|y" (alternation)
            Test("isMatch('abacaxc', 'ab.+?c')", true), // "+?" (lazy versions)
            Test("isMatch('abacaxc', 'ab.*?c')", true), // "*?" (lazy versions)
            Test("isMatch('abacaxc', 'ab.??c')", true), // "??" (lazy versions)
            Test("isMatch('12abc34', '([0-9]+)([a-z]+)([0-9]+)')", true), // "(...)" (simple group)
            Test("isMatch('12abc', '([0-9]+)([a-z]+)([0-9]+)')", false), // "(...)" (simple group)
            Test(@"isMatch('a', '\\w{1}')", true), // "\w" (match [a-zA-Z0-9_])
            Test(@"isMatch('1', '\\d{1}')", true), // "\d" (match [0-9])
            #endregion

            #region Empty expression
            Test(string.Empty, string.Empty),
            Test(string.Empty, string.Empty),
            #endregion
        };

        public static object[] Test(string input, object value, HashSet<string> paths = null) => new object[] { input, value, paths };

        public static bool IsNumber(object value) =>
           value is sbyte
           || value is byte
           || value is short
           || value is ushort
           || value is int
           || value is uint
           || value is long
           || value is ulong
           || value is float
           || value is double
           || value is decimal;

        [DataTestMethod]
        [DynamicData(nameof(Data))]
        public void Evaluate(string input, object expected, HashSet<string> expectedRefs)
        {
            var parsed = new ExpressionEngine().Parse(input);
            Assert.IsNotNull(parsed);
            var (actual, msg) = parsed.TryEvaluate(scope);
            Assert.AreEqual(null, msg);
            AssertObjectEquals(expected, actual);
            if (expectedRefs != null)
            {
                var actualRefs = parsed.References();
                Assert.IsTrue(expectedRefs.SetEquals(actualRefs), $"References do not match, expected: {string.Join(',', expectedRefs)} acutal: {string.Join(',', actualRefs)}");
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(Data))]
        public void EvaluateJson(string input, object expected, HashSet<string> expectedRefs)
        {
            var jsonScope = JToken.FromObject(scope);
            var parsed = new ExpressionEngine().Parse(input);
            Assert.IsNotNull(parsed);
            var (actual, msg) = parsed.TryEvaluate(jsonScope);
            Assert.AreEqual(null, msg);
            AssertObjectEquals(expected, actual);
            if (expectedRefs != null)
            {
                var actualRefs = parsed.References();
                Assert.IsTrue(expectedRefs.SetEquals(actualRefs), $"References do not match, expected: {string.Join(',', expectedRefs)} acutal: {string.Join(',', actualRefs)}");
            }
        }

        [TestMethod]
        public void TestAccumulatePath()
        {
            var memory = new SimpleObjectMemory(new
            {
                f = "foo",
                b = "bar",
                z = new
                {
                    z = "zar"
                },
                n = 2
            });

            var parser = new ExpressionEngine();

            // normal case, note, we doesn't append a " yet
            var exp = parser.Parse("a[f].b[n].z");
            var (path, left, err) = BuiltInFunctions.TryAccumulatePath(exp, memory);
            Assert.AreEqual(path, "a['foo'].b[2].z");

            // normal case
            exp = parser.Parse("a[z.z][z.z].y");
            (path, left, err) = BuiltInFunctions.TryAccumulatePath(exp, memory);
            Assert.AreEqual(path, "a['zar']['zar'].y");

            // normal case
            exp = parser.Parse("a.b[z.z]");
            (path, left, err) = BuiltInFunctions.TryAccumulatePath(exp, memory);
            Assert.AreEqual(path, "a.b['zar']");

            // stop evaluate at middle
            exp = parser.Parse("json(x).b");
            (path, left, err) = BuiltInFunctions.TryAccumulatePath(exp, memory);
            Assert.AreEqual(path, "b");
        }

        [TestMethod]
        public void TestConcurrency()
        {
            var input = "rand(1, 2)";
            var expected = 1;
            var parsed = new ExpressionEngine().Parse(input);
            Assert.IsNotNull(parsed);

            var t = new Thread(() =>
            {
                var (actual, msg) = parsed.TryEvaluate(new object());
                Assert.AreEqual(null, msg);
                AssertObjectEquals(expected, actual);
            });

            t.Start();
            var (actual, msg) = parsed.TryEvaluate(new object());
            Assert.AreEqual(null, msg);
            AssertObjectEquals(expected, actual);

            t.Join();
        }

        private void AssertObjectEquals(object expected, object actual)
        {
            if (IsNumber(actual) && IsNumber(expected))
            {
                if (actual is int)
                {
                    Assert.IsTrue(expected is int);
                    Assert.AreEqual(expected, actual);
                }
                else
                {
                    Assert.IsTrue(Convert.ToSingle(actual) == Convert.ToSingle(expected));
                }
            }

            // Compare two lists
            else if (expected is IList expectedList
                && actual is IList actualList)
            {
                Assert.AreEqual(expectedList.Count, actualList.Count);
                for (var i = 0; i < expectedList.Count; i++)
                {
                    Assert.AreEqual(ResolveValue(expectedList[i]), ResolveValue(actualList[i]));
                }
            }
            else
            {
                Assert.AreEqual(expected, actual);
            }
        }

        private object ResolveValue(object obj)
        {
            object value;
            if (!(obj is JValue jval))
            {
                value = obj;
            }
            else
            {
                value = jval.Value;
                if (jval.Type == JTokenType.Integer)
                {
                    value = jval.ToObject<int>();
                }
                else if (jval.Type == JTokenType.String)
                {
                    value = jval.ToObject<string>();
                }
                else if (jval.Type == JTokenType.Boolean)
                {
                    value = jval.ToObject<bool>();
                }
                else if (jval.Type == JTokenType.Float)
                {
                    value = jval.ToObject<float>();
                }
            }

            return value;
        }
    }
}
