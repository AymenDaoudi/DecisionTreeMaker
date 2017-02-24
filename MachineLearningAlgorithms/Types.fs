module Types

type node = {
               content : string;
               branch : string option;
               children : list<node> option;
               isLeaf : bool
            }

