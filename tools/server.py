from flask import Flask
import spacy
import os

from common import common

app = Flask(__name__)
nlp = None

@app.route('/recognize/<query>')
def recognize(query):
    doc = nlp(query)
    return {
        "cats": doc.cats
    }

if __name__ == '__main__':
    nlp = spacy.load(common.OutputCategoryDir)
    app.run()
