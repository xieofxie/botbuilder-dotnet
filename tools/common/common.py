#!/usr/bin/env python
# coding: utf8

import os
import json

Root = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
OutputEntityDir = os.path.join(Root, "tools\\output-entity")
OutputCategoryDir = os.path.join(Root, "tools\\output-category")

def load_json():
    JsonFile = os.path.join(Root, "tests\\Microsoft.Bot.Builder.TestBot.Json\\Samples\\ToDoLuisBot\\ToDoLuis.json")
    if os.path.exists(JsonFile):
        os.remove(JsonFile)

    LuFile = os.path.join(Root, "tests\\Microsoft.Bot.Builder.TestBot.Json\\Samples\\ToDoLuisBot\\ToDoLuis.lu")
    cmd = "bf luis:convert --in {0} --out {1}".format(LuFile, JsonFile)
    os.system(cmd)

    with open(JsonFile) as f:
        return json.load(f)
