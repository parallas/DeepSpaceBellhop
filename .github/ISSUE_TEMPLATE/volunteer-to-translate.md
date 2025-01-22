---
name: Volunteer to translate
about: "Submit localization data to translate the game, use /Content/localizations for reference"
title: "[LOCALE] Name (xx-xx)"
labels: localization
assignees: ''
body:
- type: input
  id: name
  attributes:
    label: "Language Name"
    description: "Written in the language followed by country id"
    placeholder: "Español (MX)"
  validations:
    required: true
- type: input
  id: identifier
  attributes:
    label: "Language ID"
    description: "Will be the name of the json file, uses ISO identifiers."
    placeholder: "es-mx"
  validations:
    required: true
body:
- type: checkboxes
  id: additional-checks
  attributes:
    label: "Other information"
    description: "You may select all that apply"
    options:
      - label: "Covers every token in the current version"
- type: textarea
  id: data
  attributes:
    label: "JSON Values"
    description: "Should only contain the content of the `values` property"
    placeholder: |
      {
          "token.example.yeah": "Si",
          ...
      }
    render: json
  validations:
    required: true
---
