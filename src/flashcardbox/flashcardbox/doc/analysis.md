# Analysis

## Structure of a flashcardbox
A flashcardbox is made up of n *bin*s. The bins are numbered 0..n-1.

Each bin contains a number of *flashcard*s.

A flashcard has two sides: *front* and *back*. A *question* is the content of the front page,
an *answer* is the content of the back page.

Flashcards are presented for memorization from bins 1..n-2. Bin 0 is used to add new flashcards
to. It's where flashcards are taken from if there is a need for new cards to keep the
learning process flowing.

Flashcards considered as solidly learned end up in bin n-1.

Bins 0 and n-1 are unbounded. Bins 1..n-2 are of different (usually growing) size. Sebastian
Leitner suggests the following:

1. 30 cards
2. 60 cards
3. 150 cards
4. 240 cards
5. 420 cards

A typical Leitner flashcardbox would be set to n=7: 5 bounded bins for flashcards to learn
plus 2 unbounded bins.

## Algorithm for flashcard selection
The next flashcard to memorize is taken from the *due bin*.

If the student knows the answer the flashcard moves forward to the next bin after the
due bin (its *source bin*). But if the student does not know the answer, it moves backward 
to bin 1. That's the basic Leitner approach.

Which bin is due is determined by these criteria:

* A bin becomes due if its capacity has been reached (or exceeded).
* A bin stays due as long as its fill level is above the *due threshold*.

Checking for the due status is done in this order:

1. Previous due bin
2. Bin 1
3. Bin n-1
4. Bin n-2
...

down to bin 2.

* If no bin has been found due, then bin 0 is made due. That means, flashcards have to
be transferred from bin 0 to bin 1 until its capacity is reached.

* If bin 0 should be due but empty, then the due status is assigned randomly to a
bin with cards in it.

With this approach the focus is kept on a certain bin for a while. There is no jumping around
between bins.

In general successful flashcards (in higher bins) are favored over new ones. Knowing their answers
should be solidified before new questions are introduced.

With the exception of flashcards which were poorly remembered (or are new) and are
sitting in bin 1.

Example bin capacities and due thresholds: 0(x,x), 1(30,3), 2(60,30), 3(150,120), 4(240,210), 5(420,390), 6(x,x).

Bins 0 and n-1 can be made "ordinary" bins like 1..n-2 by assigning them
special values for capacity and due threshold:

* Bin 0: capacity 0, due threshold ∞. Since the capacity is exceeded even with
just one card in it, the bin get selected if no other bin was fou‚nd due.
Likewise the bin never stays selected, because its due threshold is so high.
* Bin n-1: capacity ∞. due threshold ∞. Since the capacity is never reached it will
never be selected as due. Only during random selection could bin n-1 become due.
But that's not so bad. Firstly the probability is low, secondly it's ok to very rarely
present cards again which were deemed solidly memorized. Just to check ;-) Thirdly
random selection should be extreme rare during an ongoing learning process where there
are always new cards to memorize in bin 0.

## Scenarios
Notation:

* Bin: i(c,t,f) = bin i (0..n-1) with capacity c, due threshold t, and fill level f.
* The due bin is denoted by prefixing it with a "!", e.g. !i(c,t,f)

* 0(x,x,7), 1(5,2,0), 2(10,8,0), 3(20,17,0), 4(x,x,0) : bin 0 is set to due
* !0(x,x,2), 1(5,2,5), 2(10,8,0), 3(20,17,0), 4(x,x,0) : bin 1 is chosen
* 0(x,x,2), !1(5,2,4), 2(10,8,1), 3(20,17,0), 4(x,x,0) : bin 1 is chosen
* 0(x,x,2), !1(5,2,1), 2(10,8,4), 3(20,17,0), 4(x,x,0) : bin 0 is chosen
* !0(x,x,0), 1(5,2,3), 2(10,8,4), 3(20,17,0), 4(x,x,0) : a random bin is chosen, e.g. 2
* 0(x,x,0), 1(5,2,3), !2(10,8,3), 3(20,17,1), 4(x,x,0) : bin 0 is chosen

* 0(x,x,8), 1(5,2,1), 2(10,8,9), 3(20,17,20), 4(x,x,27) : bin 3 is chosen
* 0(x,x,8), 1(5,2,1), 2(10,8,10), !3(20,17,19), 4(x,x,27) : bin 3 is chosen
* 0(x,x,8), 1(5,2,1), 2(10,8,10), 3(20,17,20), 4(x,x,27) : bin 3 is chosen
* 0(x,x,8), 1(5,2,1), 2(10,8,10), 3(20,17,19), 4(x,x,27) : bin 2 is chosen

* 0(x,x,8), 1(5,2,1), !2(10,8,8), 3(20,17,21), 4(x,x,27) : bin 2 is chosen
* 0(x,x,8), 1(5,2,1), !2(10,8,7), 3(20,17,22), 4(x,x,27) : bin 3 is chosen

## Functions
```
interface ITeacher {
    public int Select_due_bin(IBinInfo[] bins);
}

interface IBinInfo {
    public bool IsDue {get;}
    public int Capacity {get;} // could be called UpperDueThreshold
    public int DueThreshold {get;} // could be called LowerDueThreshold‚
    public int Count {get;}
}
```