#!/bin/bash
for filename in *.yaml; do
    kubectl apply -f "$filename"
done