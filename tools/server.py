from flask import Flask
import spacy
import os

Root = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..")
OutputDir = os.path.join(Root, "tools\\output")

app = Flask(__name__)
nlp = None

@app.route('/recognize/<query>')
def recognize(query):
    doc = nlp(query)
    return {
        "cats": doc.cats
    }

if __name__ == '__main__':
    nlp = spacy.load(OutputDir)
    app.run()
