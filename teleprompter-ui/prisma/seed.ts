import { PrismaClient } from '@prisma/client'

const prisma = new PrismaClient()

async function main() {
  console.log('Seeding database...')

  // Limpar tabelas existentes
  await prisma.songFolder.deleteMany()
  await prisma.song.deleteMany()
  await prisma.folder.deleteMany()

  // 1. Criar Pastas
  const cultosFolder = await prisma.folder.create({
    data: { name: 'Cultos de Domingo', icon: 'Sun' }
  })

  const ensaiosFolder = await prisma.folder.create({
    data: { name: 'Ensaios', icon: 'Music' }
  })

  // 2. Criar Músicas
  const song1 = await prisma.song.create({
    data: {
      title: 'Amazing Grace',
      artist: 'John Newton',
      album: 'Hymns',
      rawContent: 'Amazing grace! How sweet the sound\nThat saved a wretch like me!\nI once was lost, but now am found;\nWas blind, but now I see.',
      transposeAmount: 0,
      scrollSpeed: 1.5,
      isFavorite: true
    }
  })

  const song2 = await prisma.song.create({
    data: {
      title: 'How Great Is Our God',
      artist: 'Chris Tomlin',
      album: 'Arriving',
      rawContent: 'The splendor of the King\nClothed in majesty\nLet all the earth rejoice\nAll the earth rejoice',
      transposeAmount: 2,
      scrollSpeed: 1.2,
      isFavorite: false
    }
  })

  const song3 = await prisma.song.create({
    data: {
      title: 'Oceanos',
      artist: 'Hillsong',
      album: 'Zion',
      rawContent: 'Tua voz me chama sobre as águas\nOnde os meus pés podem falhar\nE ali te encontro no mistério\nEm oceanos profundos\nMinha fé vai ficar',
      transposeAmount: -1,
      scrollSpeed: 1.0,
      isFavorite: true
    }
  })

  // 3. Vincular Músicas às Pastas (SongFolder)
  await prisma.songFolder.createMany({
    data: [
      { songId: song1.id, folderId: cultosFolder.id, order: 1 },
      { songId: song2.id, folderId: cultosFolder.id, order: 2 },
      { songId: song3.id, folderId: ensaiosFolder.id, order: 1 }
    ]
  })

  console.log('Database seeded successfully!')
}

main()
  .catch((e) => {
    console.error(e)
    process.exit(1)
  })
  .finally(async () => {
    await prisma.$disconnect()
  })
