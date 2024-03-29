WPF-приложение для синхронизации директорий, в том числе на разных носителях.

Укаызвается список соответствий директорий, между которыми будет осуществляться синхронизация. Например:
c:\Programms	d:\Programms
c:\Movies		f:\Videos

Программа строит дерево файлов для каждой директории. Одни деревья в левой части окна программы, соответсвующие им директории, с которыми будет осуществляться синхронизация - в правой.
Для дерева считывается следующая информация:
	1) наименование файла/папки;
	2) дата последнего обновления файла/папки;
	3) полный путь.

У каждого элемента дерева есть свой статус, имеющий одно из следующийх значений:
	- есть;
	- нет;
	- новый;
	- старый;
	- идентично;
	- неизвестно.
У каждого элемента отображается соответствуюший его статусу значок.

Количество элеметов дерева для синхронизируемых директорий между собой одинаковое.
Если какого-то файла/папки в одной из директрий не хватает, то в дереве создаётся на него элемент, со статусом "нет". В соответствующей синхронизируемой директории этому элементу присваивается статус "есть".
Если у файлов разные даты последнего обновления, то более раннему присваивается стутус "старый", а более новому - "новый".
Если файлы идентичны, то статусы их элементов - "идентично".
Если в директорию входят элементы с одинаковыми статусами, то и директории присваивается тот же статус. Иначе ей присваивается статус "неизвестно".

Все директории первоначально свёрнуты.

Для каждого файла, имеющего статус отличный от "идентично", отображается кнопка для подтверждения актуальности его наличия/отсутствия/отличия.
При нажатии на кнопку в синхронизируемой директории осуществляются соответствующие действия: добавление файла, его замена или удаление.
Для каждой директории, имеющей статус отличный от "идентично" или "неизвестно", так же отображается кнопка для подтверждения её актуальности.
После выполнения действий по актуализации статусы обновлённых файлов и директорий (если была нажата кнопка для директории) обновляются.
Актуализация происходит в отдельном потоке. Поэтому можно параллельно запустить синхронизацию нескольких директорий. У синхронизируемых элементов отображается значок, информирующий о процессе синхронизации.
