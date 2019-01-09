import os
import codecs
from src.landfill import LandfillAlgorithm

def main():
    filename = '../../test/small.csv'
    file = codecs.open(filename, mode="rb")
    in_file = {'file': file}
    alg = LandfillAlgorithm()
    excel = alg.generate_report(in_file)
    outfile = open('./test.xlsx','wb')
    excel.seek(0)
    for thing in excel:
        outfile.write(thing)

    print(alg)



if __name__ == '__main__':
    main()