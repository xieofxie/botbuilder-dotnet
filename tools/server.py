from flask import Flask
import spacy
import os

from common import common

app = Flask(__name__)
nlp_category = None
nlp_entity = None

@app.route('/recognize/<query>')
def recognize(query):
    doc_category = nlp_category(query)
    doc_entity = nlp_entity(query)

    return {
        "cats": doc_category.cats,
        "ents": [(ent.label_, ent.text) for ent in doc_entity.ents],
    }

if __name__ == '__main__':
    nlp_category = spacy.load(common.OutputCategoryDir)
    nlp_entity = spacy.load(common.OutputEntityDir)
    app.run()
